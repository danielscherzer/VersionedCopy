namespace VersionedCopy.Interfaces
{
	public interface IDirectories
	{
		string DestinationDirectory { get; }
		string OldFilesFolder { get; }
		string SourceDirectory { get; }
	}
}