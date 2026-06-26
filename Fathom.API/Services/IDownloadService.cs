using System;
using System.Collections.Generic;
using Fathom.Models.Entities;

namespace Fathom.API.Services;

public interface IDownloadService
{
    Tuple<string, string, string> GetFirstFileDownload(IEnumerable<MangaFile> files);
    string GetContentTypeFromFile(string filepath);
}
