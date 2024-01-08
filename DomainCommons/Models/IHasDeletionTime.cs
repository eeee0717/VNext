namespace DomainCommons.Models;

public interface IHasDeletionTime
{
  DateTime? DeletionTime { get; }

}