using System.Collections.Generic;
using System.Threading.Tasks;

namespace FFgui.Services;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickFilesAsync();
}