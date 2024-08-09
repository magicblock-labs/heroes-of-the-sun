using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using Upgrade;
using Upgrade.Program;
using Upgrade.Errors;
using Upgrade.Accounts;
using Upgrade.Types;

namespace Upgrade
{
    namespace Accounts
    {
        public partial class SettlementComponent
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 12867611429926918130UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{242, 215, 140, 132, 107, 240, 146, 178};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "hcsYewA26N5";
            public Building[] Buildings { get; set; }

            public ResourceBalance Environment { get; set; }

            public ResourceBalance Treasury { get; set; }

            public ushort Day { get; set; }

            public byte Faith { get; set; }

            public sbyte[] LabourAllocation { get; set; }

            public BoltMetadata BoltMetadata { get; set; }

            public static SettlementComponent Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                SettlementComponent result = new SettlementComponent();
                int resultBuildingsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Buildings = new Building[resultBuildingsLength];
                for (uint resultBuildingsIdx = 0; resultBuildingsIdx < resultBuildingsLength; resultBuildingsIdx++)
                {
                    offset += Building.Deserialize(_data, offset, out var resultBuildingsresultBuildingsIdx);
                    result.Buildings[resultBuildingsIdx] = resultBuildingsresultBuildingsIdx;
                }

                offset += ResourceBalance.Deserialize(_data, offset, out var resultEnvironment);
                result.Environment = resultEnvironment;
                offset += ResourceBalance.Deserialize(_data, offset, out var resultTreasury);
                result.Treasury = resultTreasury;
                result.Day = _data.GetU16(offset);
                offset += 2;
                result.Faith = _data.GetU8(offset);
                offset += 1;
                int resultLabourAllocationLength = (int)_data.GetU32(offset);
                offset += 4;
                result.LabourAllocation = new sbyte[resultLabourAllocationLength];
                for (uint resultLabourAllocationIdx = 0; resultLabourAllocationIdx < resultLabourAllocationLength; resultLabourAllocationIdx++)
                {
                    result.LabourAllocation[resultLabourAllocationIdx] = _data.GetS8(offset);
                    offset += 1;
                }

                offset += BoltMetadata.Deserialize(_data, offset, out var resultBoltMetadata);
                result.BoltMetadata = resultBoltMetadata;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum UpgradeErrorKind : uint
        {
            BuildingIndexOutOfRange = 6000U,
            NotEnoughResources = 6001U
        }
    }

    namespace Types
    {
        public partial class BoltMetadata
        {
            public PublicKey Authority { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WritePubKey(Authority, offset);
                offset += 32;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out BoltMetadata result)
            {
                int offset = initialOffset;
                result = new BoltMetadata();
                result.Authority = _data.GetPubKey(offset);
                offset += 32;
                return offset - initialOffset;
            }
        }

        public partial class Building
        {
            public byte X { get; set; }

            public byte Y { get; set; }

            public byte Deterioration { get; set; }

            public BuildingType Id { get; set; }

            public byte Level { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU8(X, offset);
                offset += 1;
                _data.WriteU8(Y, offset);
                offset += 1;
                _data.WriteU8(Deterioration, offset);
                offset += 1;
                _data.WriteU8((byte)Id, offset);
                offset += 1;
                _data.WriteU8(Level, offset);
                offset += 1;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out Building result)
            {
                int offset = initialOffset;
                result = new Building();
                result.X = _data.GetU8(offset);
                offset += 1;
                result.Y = _data.GetU8(offset);
                offset += 1;
                result.Deterioration = _data.GetU8(offset);
                offset += 1;
                result.Id = (BuildingType)_data.GetU8(offset);
                offset += 1;
                result.Level = _data.GetU8(offset);
                offset += 1;
                return offset - initialOffset;
            }
        }

        public enum BuildingType : byte
        {
            TownHall,
            WaterCollector,
            FoodCollector,
            WoodCollector,
            WaterStorage,
            FoodStorage,
            WoodStorage,
            Altar
        }

        public partial class ResourceBalance
        {
            public ushort Food { get; set; }

            public ushort Water { get; set; }

            public ushort Wood { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU16(Food, offset);
                offset += 2;
                _data.WriteU16(Water, offset);
                offset += 2;
                _data.WriteU16(Wood, offset);
                offset += 2;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out ResourceBalance result)
            {
                int offset = initialOffset;
                result = new ResourceBalance();
                result.Food = _data.GetU16(offset);
                offset += 2;
                result.Water = _data.GetU16(offset);
                offset += 2;
                result.Wood = _data.GetU16(offset);
                offset += 2;
                return offset - initialOffset;
            }
        }
    }

    public partial class UpgradeClient : TransactionalBaseClient<UpgradeErrorKind>
    {
        public UpgradeClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SettlementComponent>>> GetSettlementComponentsAsync(string programAddress, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = SettlementComponent.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SettlementComponent>>(res);
            List<SettlementComponent> resultingAccounts = new List<SettlementComponent>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => SettlementComponent.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SettlementComponent>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<SettlementComponent>> GetSettlementComponentAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<SettlementComponent>(res);
            var resultingAccount = SettlementComponent.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<SettlementComponent>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeSettlementComponentAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, SettlementComponent> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                SettlementComponent parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = SettlementComponent.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<UpgradeErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<UpgradeErrorKind>>{{6000U, new ProgramError<UpgradeErrorKind>(UpgradeErrorKind.BuildingIndexOutOfRange, "Supplied Building index out range")}, {6001U, new ProgramError<UpgradeErrorKind>(UpgradeErrorKind.NotEnoughResources, "Not Enough Resources")}, };
        }
    }

    namespace Program
    {
        public class ExecuteAccounts
        {
            public PublicKey Settlement { get; set; }

            public PublicKey Authority { get; set; }
        }

        public static class UpgradeProgram
        {
            public const string ID = "11111111111111111111111111111111";
            public static Solana.Unity.Rpc.Models.TransactionInstruction Execute(ExecuteAccounts accounts, byte[] _args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Settlement, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2143081261876567426UL, offset);
                offset += 8;
                _data.WriteS32(_args.Length, offset);
                offset += 4;
                _data.WriteSpan(_args, offset);
                offset += _args.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}