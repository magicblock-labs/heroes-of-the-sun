using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Solana.Unity.Wallet;
using World;

namespace Connectors
{
    public class Pda
    {
        public static PublicKey FindWorldPda(int world)
        {
            PublicKey.TryFindProgramAddress(new[]
            {
                Encoding.UTF8.GetBytes("world"), BitConverter.GetBytes((ulong)world).Reverse().ToArray()
            }, WorldClient.ProgramID, out var pda, out _);
            return pda;
        }

        public static PublicKey FindEntityPda(int world, int entity, string extraSeed = "")
        {
            PublicKey.TryFindProgramAddress(new[]
            {
                Encoding.UTF8.GetBytes("entity"), BitConverter.GetBytes((ulong)world).Reverse().ToArray(),
                BitConverter.GetBytes((ulong)entity).Reverse().ToArray(), Encoding.UTF8.GetBytes(extraSeed)
            }, WorldClient.ProgramID, out var pda, out _);
            return pda;
        }

        public static PublicKey FindComponentPda(PublicKey entity, PublicKey componentProgram)
        {
            return FindComponentPda(null, entity, componentProgram);
        }

        private static PublicKey FindComponentPda(
            [CanBeNull] string componentId,
            PublicKey entity,
            PublicKey componentProgram)
        {
            if (string.IsNullOrEmpty(componentId))
            {
                PublicKey.TryFindProgramAddress(new[]
                {
                    entity.KeyBytes
                }, componentProgram, out var pda, out _);
                return pda;
            }
            else
            {
                PublicKey.TryFindProgramAddress(new[]
                {
                    Encoding.UTF8.GetBytes(componentId), entity.KeyBytes
                }, componentProgram, out var pda, out _);
                return pda;
            }
        }
    }
}