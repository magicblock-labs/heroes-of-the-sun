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
using Settlement;
using Settlement.Program;
using Settlement.Errors;
using Settlement.Accounts;
using Settlement.Types;

namespace Settlement
{
    namespace Accounts
    {
        public partial class Entity
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 1751670451238706478UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{46, 157, 161, 161, 254, 46, 79, 24};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "8oEQa6zH67R";
            public ulong Id { get; set; }

            public static Entity Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Entity result = new Entity();
                result.Id = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }

        public partial class Settlement
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 13125890802739514167UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{55, 11, 219, 33, 36, 136, 40, 182};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "AD24AwEsvU5";
            public Building[] Buildings { get; set; }

            public ResourceBalance Environment { get; set; }

            public ResourceBalance Treasury { get; set; }

            public byte Faith { get; set; }

            public byte TimeUnits { get; set; }

            public long LastTimeClaim { get; set; }

            public uint Research { get; set; }

            public sbyte[] WorkerAssignment { get; set; }

            public BoltMetadata BoltMetadata { get; set; }

            public static Settlement Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Settlement result = new Settlement();
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
                result.Faith = _data.GetU8(offset);
                offset += 1;
                result.TimeUnits = _data.GetU8(offset);
                offset += 1;
                result.LastTimeClaim = _data.GetS64(offset);
                offset += 8;
                result.Research = _data.GetU32(offset);
                offset += 4;
                int resultWorkerAssignmentLength = (int)_data.GetU32(offset);
                offset += 4;
                result.WorkerAssignment = new sbyte[resultWorkerAssignmentLength];
                for (uint resultWorkerAssignmentIdx = 0; resultWorkerAssignmentIdx < resultWorkerAssignmentLength; resultWorkerAssignmentIdx++)
                {
                    result.WorkerAssignment[resultWorkerAssignmentIdx] = _data.GetS8(offset);
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
        public enum SettlementErrorKind : uint
        {
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

            public byte TurnsToBuild { get; set; }

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
                _data.WriteU8(TurnsToBuild, offset);
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
                result.TurnsToBuild = _data.GetU8(offset);
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

    public partial class SettlementClient : TransactionalBaseClient<SettlementErrorKind>
    {
        public SettlementClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>> GetEntitysAsync(string programAddress, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Entity.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res);
            List<Entity> resultingAccounts = new List<Entity>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Entity.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Settlement.Accounts.Settlement>>> GetSettlementsAsync(string programAddress, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Settlement.Accounts.Settlement.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Settlement.Accounts.Settlement>>(res);
            List<Settlement.Accounts.Settlement> resultingAccounts = new List<Settlement.Accounts.Settlement>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Settlement.Accounts.Settlement.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Settlement.Accounts.Settlement>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Entity>> GetEntityAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Entity>(res);
            var resultingAccount = Entity.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Entity>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Settlement.Accounts.Settlement>> GetSettlementAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Settlement.Accounts.Settlement>(res);
            var resultingAccount = Settlement.Accounts.Settlement.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Settlement.Accounts.Settlement>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeEntityAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Entity> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Entity parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Entity.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeSettlementAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Settlement.Accounts.Settlement> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Settlement.Accounts.Settlement parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Settlement.Accounts.Settlement.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<SettlementErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<SettlementErrorKind>>{};
        }
    }

    namespace Program
    {
        public class InitializeAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Data { get; set; }

            public PublicKey Entity { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class UpdateAccounts
        {
            public PublicKey BoltComponent { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }
        }

        public static class SettlementProgram
        {
            public const string ID = "11111111111111111111111111111111";
            public static Solana.Unity.Rpc.Models.TransactionInstruction Initialize(InitializeAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Data, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Entity, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(17121445590508351407UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Update(UpdateAccounts accounts, byte[] data, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(9222597562720635099UL, offset);
                offset += 8;
                _data.WriteS32(data.Length, offset);
                offset += 4;
                _data.WriteSpan(data, offset);
                offset += data.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}