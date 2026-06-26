using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.KavitaPlus.Metadata;
using Fathom.Models.DTOs.Settings;
using Fathom.Models.Entities;
using Fathom.Models.Entities.Enums;
using Fathom.Models.Entities.Metadata;
using Fathom.Models.Entities.MetadataMatching;

namespace Fathom.API.Repositories;

public interface ISettingsRepository
{
    void Update(ServerSetting settings);
    void Update(MetadataSettings settings);
    void RemoveRange(List<MetadataFieldMapping> fieldMappings);
    Task<ServerSettingDto> GetSettingsDtoAsync(CancellationToken ct = default);
    Task<ServerSetting> GetSettingAsync(ServerSettingKey key, CancellationToken ct = default);
    Task<IEnumerable<ServerSetting>> GetSettingsAsync(CancellationToken ct = default);
    Task<MetadataSettings> GetMetadataSettings(CancellationToken ct = default);
    Task<MetadataSettingsDto> GetMetadataSettingDto(CancellationToken ct = default);
}
