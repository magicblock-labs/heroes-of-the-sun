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

        public partial class SessionToken
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 1081168673100727529UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{233, 4, 115, 14, 46, 21, 1, 15};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "fyZWTdUu1pS";
            public PublicKey Authority { get; set; }

            public PublicKey TargetProgram { get; set; }

            public PublicKey SessionSigner { get; set; }

            public long ValidUntil { get; set; }

            public static SessionToken Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                SessionToken result = new SessionToken();
                result.Authority = _data.GetPubKey(offset);
                offset += 32;
                result.TargetProgram = _data.GetPubKey(offset);
                offset += 32;
                result.SessionSigner = _data.GetPubKey(offset);
                offset += 32;
                result.ValidUntil = _data.GetS64(offset);
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

            public PublicKey Owner { get; set; }

            public EnvironmentState Environment { get; set; }

            public ResourceBalance Treasury { get; set; }

            public byte Faith { get; set; }

            public byte TimeUnits { get; set; }

            public long LastTimeClaim { get; set; }

            public uint Research { get; set; }

            public sbyte[] WorkerAssignment { get; set; }

            public ulong QuestClaimStatus { get; set; }

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

                result.Owner = _data.GetPubKey(offset);
                offset += 32;
                offset += EnvironmentState.Deserialize(_data, offset, out var resultEnvironment);
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

                result.QuestClaimStatus = _data.GetU64(offset);
                offset += 8;
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

            public ushort Extraction { get; set; }

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
                _data.WriteU16(Extraction, offset);
                offset += 2;
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
                result.Extraction = _data.GetU16(offset);
                offset += 2;
                return offset - initialOffset;
            }
        }

        public enum BuildingType : byte
        {
            TownHall,
            Altar,
            Research,
            FoodCollector,
            FoodStorage,
            WoodCollector,
            WoodStorage,
            WaterCollector,
            WaterStorage,
            StoneCollector,
            StoneStorage
        }

        public partial class EnvironmentState
        {
            public ushort Food { get; set; }

            public ushort Wood { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU16(Food, offset);
                offset += 2;
                _data.WriteU16(Wood, offset);
                offset += 2;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out EnvironmentState result)
            {
                int offset = initialOffset;
                result = new EnvironmentState();
                result.Food = _data.GetU16(offset);
                offset += 2;
                result.Wood = _data.GetU16(offset);
                offset += 2;
                return offset - initialOffset;
            }
        }

        public partial class ResourceBalance
        {
            public ushort Food { get; set; }

            public ushort Water { get; set; }

            public ushort Wood { get; set; }

            public ushort Stone { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU16(Food, offset);
                offset += 2;
                _data.WriteU16(Water, offset);
                offset += 2;
                _data.WriteU16(Wood, offset);
                offset += 2;
                _data.WriteU16(Stone, offset);
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
                result.Stone = _data.GetU16(offset);
                offset += 2;
                return offset - initialOffset;
            }
        }
    }

    public partial class SettlementClient : TransactionalBaseClient<SettlementErrorKind>
    {
        public SettlementClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId = null) : base(rpcClient, streamingRpcClient, programId ?? new PublicKey(SettlementProgram.ID))
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>> GetEntitysAsync(string programAddress = SettlementProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Entity.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res);
            List<Entity> resultingAccounts = new List<Entity>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Entity.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SessionToken>>> GetSessionTokensAsync(string programAddress = SettlementProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = SessionToken.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SessionToken>>(res);
            List<SessionToken> resultingAccounts = new List<SessionToken>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => SessionToken.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<SessionToken>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Settlement.Accounts.Settlement>>> GetSettlementsAsync(string programAddress = SettlementProgram.ID, Commitment commitment = Commitment.Confirmed)
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

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<SessionToken>> GetSessionTokenAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<SessionToken>(res);
            var resultingAccount = SessionToken.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<SessionToken>(res, resultingAccount);
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

        public async Task<SubscriptionState> SubscribeSessionTokenAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, SessionToken> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                SessionToken parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = SessionToken.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
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
        public class DelegateAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Entity { get; set; }

            public PublicKey Account { get; set; }

            public PublicKey OwnerProgram { get; set; }

            public PublicKey Buffer { get; set; }

            public PublicKey DelegationRecord { get; set; }

            public PublicKey DelegationMetadata { get; set; }

            public PublicKey DelegationProgram { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class DestroyAccounts
        {
            public PublicKey Authority { get; set; }

            public PublicKey Receiver { get; set; }

            public PublicKey Entity { get; set; }

            public PublicKey Component { get; set; }

            public PublicKey ComponentProgramData { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; } = new PublicKey("Sysvar1nstructions1111111111111111111111111");
            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
        }

        public class InitializeAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Data { get; set; }

            public PublicKey Entity { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; } = new PublicKey("Sysvar1nstructions1111111111111111111111111");
            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
        }

        public class ProcessUndelegationAccounts
        {
            public PublicKey DelegatedAccount { get; set; }

            public PublicKey Buffer { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class UndelegateAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey DelegatedAccount { get; set; }

            public PublicKey MagicContext { get; set; } = new PublicKey("MagicContext1111111111111111111111111111111");
            public PublicKey MagicProgram { get; set; } = new PublicKey("Magic11111111111111111111111111111111111111");
        }

        public class UpdateAccounts
        {
            public PublicKey BoltComponent { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; } = new PublicKey("Sysvar1nstructions1111111111111111111111111");
        }

        public class UpdateWithSessionAccounts
        {
            public PublicKey BoltComponent { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; } = new PublicKey("Sysvar1nstructions1111111111111111111111111");
            public PublicKey SessionToken { get; set; }
        }

        public static class SettlementProgram
        {
            public const string ID = "5bKBE1HgusXC5jVVjpk4CvxUM8UGnVPQyvGt7cB6Jk7W";
            public static Solana.Unity.Rpc.Models.TransactionInstruction Delegate(DelegateAccounts accounts, uint commit_frequency_ms, PublicKey validator, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Entity, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Account, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.OwnerProgram, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Buffer, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.DelegationRecord, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.DelegationMetadata, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.DelegationProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(9873113408189731674UL, offset);
                offset += 8;
                _data.WriteU32(commit_frequency_ms, offset);
                offset += 4;
                if (validator != null)
                {
                    _data.WriteU8(1, offset);
                    offset += 1;
                    _data.WritePubKey(validator, offset);
                    offset += 32;
                }
                else
                {
                    _data.WriteU8(0, offset);
                    offset += 1;
                }

                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Destroy(DestroyAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Receiver, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Entity, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Component, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgramData, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5372736661213948061UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Initialize(InitializeAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
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

            public static Solana.Unity.Rpc.Models.TransactionInstruction ProcessUndelegation(ProcessUndelegationAccounts accounts, byte[][] account_seeds, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.DelegatedAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Buffer, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(12048014319693667524UL, offset);
                offset += 8;
                _data.WriteS32(account_seeds.Length, offset);
                offset += 4;
                foreach (var account_seedsElement in account_seeds)
                {
                    _data.WriteS32(account_seedsElement.Length, offset);
                    offset += 4;
                    _data.WriteSpan(account_seedsElement, offset);
                    offset += account_seedsElement.Length;
                }

                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Undelegate(UndelegateAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.DelegatedAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MagicContext, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.MagicProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(17161644073433732227UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Update(UpdateAccounts accounts, byte[] data, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
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

            public static Solana.Unity.Rpc.Models.TransactionInstruction UpdateWithSession(UpdateWithSessionAccounts accounts, byte[] data, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SessionToken, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(13131745794163226589UL, offset);
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