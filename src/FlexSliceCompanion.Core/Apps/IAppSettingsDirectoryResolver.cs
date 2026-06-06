namespace FlexSliceCompanion.Core.Apps;

public interface IAppSettingsDirectoryResolver
{
    string ResolveSettingsDirectory(string root, string sliceLetter);
}
