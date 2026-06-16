using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Abstractions.Loadouts.Synchronizers.Rules;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;

namespace NexusMods.Games.FileHashes.Emitters;

public class UndeployableLoadoutDueToMissingGameFiles : ILoadoutDiagnosticEmitter
{
    private readonly IConnection _connection;

    public UndeployableLoadoutDueToMissingGameFiles(IServiceProvider serviceProvider)
    {
        _connection = serviceProvider.GetRequiredService<IConnection>();
    }

    private static bool IsOptionalGogGalaxyMetadataPath(GamePath path)
    {
        if (path.LocationId != LocationId.Game)
            return false;

        var value = path.Path.ToString();

        if (!value.StartsWith("goggame-", StringComparison.OrdinalIgnoreCase))
            return false;

        return value.EndsWith(".info", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".hashdb", StringComparison.OrdinalIgnoreCase);
    }

    public IAsyncEnumerable<Diagnostic> Diagnose(Loadout.ReadOnly loadout, CancellationToken cancellationToken) => throw new NotSupportedException();
    public async IAsyncEnumerable<Diagnostic> Diagnose(Loadout.ReadOnly loadout, FrozenDictionary<GamePath, SyncNode> syncTree, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();

        var sb = new StringBuilder();

        var totalSize = Size.Zero;
        var count = 0;

        foreach (var (gamePath, node) in syncTree)
        {
            if (node.SourceItemType is not LoadoutSourceItemType.Game || !node.Actions.HasFlag(Actions.WarnOfUnableToExtract)) continue;
            if (IsOptionalGogGalaxyMetadataPath(gamePath)) continue;

            totalSize += node.Loadout.Size;
            count++;

            sb.AppendLine($"* `{gamePath}`");
        }

        if (count > 0)
        {
            yield return Diagnostics.CreateUndeployableLoadoutDueToMissingGameFiles(
                Size: totalSize,
                FileCount: count,
                Game: loadout.InstallationInstance.Game.DisplayName,
                Files: sb.ToString(),
                Store: loadout.InstallationInstance.Store.Value,
                Version: loadout.GameVersion.ToString()
            );
        }
    }
}
