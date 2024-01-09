namespace VNext.DomainCommons.Models;

public interface IHasDeletionTime
{
  DateTime? DeletionTime { get; }

}