using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace VNext.Commons;

public static class ReflectionHelper
{
  /// <summary>
  /// 判断是否为微软程序集
  /// </summary>
  /// <param name="asm"></param>
  /// <returns></returns>
  private static bool IsSystemAssembly(Assembly asm)
  {
    var asmCompanyAttr = asm.GetCustomAttribute<AssemblyCompanyAttribute>();
    if (asmCompanyAttr == null)
    {
      return false;
    }

    string companyName = asmCompanyAttr.Company;
    return companyName.Contains("Microsoft");
  }
  
  private static bool IsSystemAssembly(string asmPath)
  {
    var moduleDef = AsmResolver.DotNet.ModuleDefinition.FromFile(asmPath);
    var assembly = moduleDef.Assembly;
    if(assembly==null)
    {
      return false;
    }
    var asmCompanyAttr = assembly.CustomAttributes.FirstOrDefault(c => c.Constructor?.DeclaringType?.FullName == typeof(AssemblyCompanyAttribute).FullName);
    if(asmCompanyAttr==null)
    {
      return false;
    }
    var companyName = ((AsmResolver.Utf8String?)asmCompanyAttr.Signature?.FixedArguments[0]?.Element)?.Value;
    if(companyName==null)
    {
      return false;
    }
    return companyName.Contains("Microsoft");
  }
  
  /// <summary>
  /// 判断file这个文件是否是程序集
  /// </summary>
  /// <param name="file"></param>
  /// <returns></returns>
  private static bool IsManagedAssembly(string file)
  {
    using var fs = File.OpenRead(file);
    using PEReader peReader = new PEReader(fs);
    return peReader.HasMetadata&&peReader.GetMetadataReader().IsAssembly;
  }

  private static bool IsValid(Assembly asm)
  {
    try
    {
      asm.GetTypes();
      // ?
      asm.DefinedTypes.ToList();
      return true;
    }
    catch (ReflectionTypeLoadException)
    {
      return false;
    }
  }
  private static Assembly? TryLoadAssembly(string asmPath)
  {
    AssemblyName asmName = AssemblyName.GetAssemblyName(asmPath);
    Assembly? asm=null;
    try
    {
      asm = Assembly.Load(asmName);
    }
    catch (BadImageFormatException ex)
    {
      Debug.WriteLine(ex);
    }
    catch (FileLoadException ex)
    {
      Debug.WriteLine(ex);
    }
        
    if (asm == null)
    {
      try
      {
        asm = Assembly.LoadFile(asmPath);
      }
      catch (BadImageFormatException ex)
      {
        Debug.WriteLine(ex);
      }
      catch (FileLoadException ex)
      {
        Debug.WriteLine(ex);
      }
    }            
    return asm;
  }

  public static IEnumerable<Assembly> GetAllReferencedAssemblies(bool skipSystemAssemblies = true)
  {
    // 获取入口程序集
    Assembly? rootAssembly = Assembly.GetEntryAssembly();
    if (rootAssembly == null)
    {
      // 如果不存在，则获取调用者的程序集
      rootAssembly = Assembly.GetCallingAssembly();
    }

    var returnAssemblies = new HashSet<Assembly>(new AssemblyEquality());
    var loadedAssemblies = new HashSet<string>();
    var assembliesToCheck = new Queue<Assembly>();
    assembliesToCheck.Enqueue(rootAssembly);
    if (skipSystemAssemblies && IsSystemAssembly(rootAssembly) != false)
    {
      if (IsValid(rootAssembly))
      {
        returnAssemblies.Add(rootAssembly);
      }
    }

    while (assembliesToCheck.Any())
    {
      var assemblyToCheck = assembliesToCheck.Dequeue();
      foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
      {
        if (!loadedAssemblies.Contains(reference.FullName))
        {
          var assembly = Assembly.Load(reference);
          if (skipSystemAssemblies && IsSystemAssembly(assembly))
          {
            continue;
          }
          assembliesToCheck.Enqueue(assembly);
          loadedAssemblies.Add(reference.FullName);
          if (IsValid(assembly))
          {
            returnAssemblies.Add(assembly);
          }
        }
      }
    }
    var asmsInBaseDir = Directory.EnumerateFiles(AppContext.BaseDirectory,
      "*.dll", new EnumerationOptions { RecurseSubdirectories = true });
    foreach (var asmPath in asmsInBaseDir)
    {
      if (!IsManagedAssembly(asmPath))
      {
        continue;
      }            
      AssemblyName asmName = AssemblyName.GetAssemblyName(asmPath);
      //如果程序集已经加载过了就不再加载
      if (returnAssemblies.Any(x => AssemblyName.ReferenceMatchesDefinition(x.GetName(), asmName)))
      {
        continue;
      }
      if (skipSystemAssemblies && IsSystemAssembly(asmPath))
      {
        continue;
      }
      Assembly? asm = TryLoadAssembly(asmPath);
      if(asm==null)
      {
        continue;
      }
      //Assembly asm = Assembly.Load(asmName);
      if (!IsValid(asm))
      {
        continue;
      }
      if (skipSystemAssemblies && IsSystemAssembly(asm))
      {
        continue;
      }
      returnAssemblies.Add(asm);
    }
    return returnAssemblies.ToArray();
  }

  /// <summary>
  /// 判断file这个文件是否是程序集
  /// </summary>
  class AssemblyEquality : EqualityComparer<Assembly>
  {
    public override bool Equals(Assembly? x, Assembly? y)
    {
      if (x == null && y == null) return true;
      if (x == null || y == null) return false;
      return AssemblyName.ReferenceMatchesDefinition(x.GetName(), y.GetName());
    }

    public override int GetHashCode(Assembly obj)
    {
      return obj.GetName().FullName.GetHashCode();
    }
  }
}