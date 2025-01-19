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
using Hero;
using Hero.Program;
using Hero.Errors;
using Hero.Accounts;
using Hero.Types;

namespace Hero
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

        public partial class Hero
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 12437505895584710412UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{12, 63, 39, 30, 183, 229, 154, 172};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "33osvPqZyNB";
            public int X { get; set; }

            public int Y { get; set; }

            public long LastActivity { get; set; }

            public PublicKey Owner { get; set; }

            public ResourceBalance Backpack { get; set; }

            public BoltMetadata BoltMetadata { get; set; }

            public static Hero Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Hero result = new Hero();
                result.X = _data.GetS32(offset);
                offset += 4;
                result.Y = _data.GetS32(offset);
                offset += 4;
                result.LastActivity = _data.GetS64(offset);
                offset += 8;
                result.Owner = _data.GetPubKey(offset);
                offset += 32;
                offset += ResourceBalance.Deserialize(_data, offset, out var resultBackpack);
                result.Backpack = resultBackpack;
                offset += BoltMetadata.Deserialize(_data, offset, out var resultBoltMetadata);
                result.BoltMetadata = resultBoltMetadata;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum HeroErrorKind : uint
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

    public partial class HeroClient : TransactionalBaseClient<HeroErrorKind>
    {
        public HeroClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId = null) : base(rpcClient, streamingRpcClient, programId ?? new PublicKey(HeroProgram.ID))
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>> GetEntitysAsync(string programAddress = HeroProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Entity.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res);
            List<Entity> resultingAccounts = new List<Entity>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Entity.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Hero.Accounts.Hero>>> GetHerosAsync(string programAddress = HeroProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Hero.Accounts.Hero.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Hero.Accounts.Hero>>(res);
            List<Hero.Accounts.Hero> resultingAccounts = new List<Hero.Accounts.Hero>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Hero.Accounts.Hero.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Hero.Accounts.Hero>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Entity>> GetEntityAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Entity>(res);
            var resultingAccount = Entity.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Entity>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Hero.Accounts.Hero>> GetHeroAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Hero.Accounts.Hero>(res);
            var resultingAccount = Hero.Accounts.Hero.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Hero.Accounts.Hero>(res, resultingAccount);
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

        public async Task<SubscriptionState> SubscribeHeroAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Hero.Accounts.Hero> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Hero.Accounts.Hero parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Hero.Accounts.Hero.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<HeroErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<HeroErrorKind>>{};
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

        public static class HeroProgram
        {
            public const string ID = "GBzY8ujNDb1FNkJUXUUjKV5uZPqzi6AoKsPjsqFEHCeh";
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