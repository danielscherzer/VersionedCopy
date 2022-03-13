using System;
using System.IO;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	internal class FileOperations
	{
		public FileOperations(IOutput errorOutput, bool readOnly)
		{
			ErrorOutput = errorOutput ?? throw new ArgumentNullException(nameof(errorOutput));
			ReadOnly = readOnly;
		}

		private IOutput ErrorOutput { get; }
		private bool ReadOnly { get; }

		public bool CreateDirectory(string path)
		{
			if (ReadOnly) return true;
			return Successful(() => Directory.CreateDirectory(path));
		}

		public bool CreateDirectory(string path, DateTime creationTimeUtc)
		{
			if (ReadOnly) return true;
			return Successful(() => Directory.CreateDirectory(path).CreationTimeUtc = creationTimeUtc);
		}

		public bool Copy(string srcFilePath, string dstFilePath)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				File.Copy(srcFilePath, dstFilePath);
			});
		}

		public bool MoveDirectory(string source, string destination)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				Directory.CreateDirectory(destination + "..");
				Directory.Move(source, destination);
			});
		}

		public bool MoveFile(string source, string destination)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				var parentDir = Path.GetDirectoryName(destination);
				if (parentDir is null)
				{
					ErrorOutput.Error($"File '{destination}' has no parent directory.");
					return;
				}
				Directory.CreateDirectory(parentDir);
				File.Move(source, destination);
			});
		}

		public bool SetTimeStamp(string filePath, DateTime newTime)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				if (Snapshot.IsFile(filePath))
				{
					File.SetLastWriteTimeUtc(filePath, newTime);
				}
				else
				{
					Directory.SetCreationTimeUtc(filePath, newTime);
				}
			});
		}

		private bool Successful(Action action)
		{
			try
			{
				action();
				return true;
			}
			catch (IOException e)
			{
				ErrorOutput.Error(e.Message);
				return false;
			}
		}
	}
}