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
using DeityBot;
using DeityBot.Program;
using DeityBot.Errors;
using DeityBot.Accounts;
using DeityBot.Types;

namespace DeityBot
{
    namespace Accounts
    {
        public partial class Agent
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 528827278246848047UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{47, 166, 112, 147, 155, 197, 86, 7};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "8yGSUtV5BEn";
            public PublicKey Context { get; set; }

            public string Name { get; set; }

            public byte Happiness { get; set; }

            public byte Trust { get; set; }

            public static Agent Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Agent result = new Agent();
                result.Context = _data.GetPubKey(offset);
                offset += 32;
                offset += _data.GetBorshString(offset, out var resultName);
                result.Name = resultName;
                result.Happiness = _data.GetU8(offset);
                offset += 1;
                result.Trust = _data.GetU8(offset);
                offset += 1;
                return result;
            }
        }

        public partial class ContextAccount
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 7879649602334994507UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{75, 176, 185, 173, 144, 35, 90, 109};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "DfHt5XjNajW";
            public string Text { get; set; }

            public static ContextAccount Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                ContextAccount result = new ContextAccount();
                offset += _data.GetBorshString(offset, out var resultText);
                result.Text = resultText;
                return result;
            }
        }

        public partial class Counter
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 1836621736066724095UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{255, 176, 4, 245, 188, 253, 124, 25};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "jmVQbGxuVYt";
            public uint Count { get; set; }

            public static Counter Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Counter result = new Counter();
                result.Count = _data.GetU32(offset);
                offset += 4;
                return result;
            }
        }

        public partial class Identity
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 8094556981291222074UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{58, 132, 5, 12, 176, 164, 85, 112};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "AngBkYjJgCF";
            public static Identity Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Identity result = new Identity();
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum DeityBotErrorKind : uint
        {
        }
    }

    namespace Types
    {
    }

    public partial class DeityBotClient : TransactionalBaseClient<DeityBotErrorKind>
    {
        public DeityBotClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId = null) : base(rpcClient, streamingRpcClient, programId ?? new PublicKey(DeityBotProgram.ID))
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Agent>>> GetAgentsAsync(string programAddress = DeityBotProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Agent.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Agent>>(res);
            List<Agent> resultingAccounts = new List<Agent>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Agent.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Agent>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<ContextAccount>>> GetContextAccountsAsync(string programAddress = DeityBotProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = ContextAccount.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<ContextAccount>>(res);
            List<ContextAccount> resultingAccounts = new List<ContextAccount>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => ContextAccount.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<ContextAccount>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Counter>>> GetCountersAsync(string programAddress = DeityBotProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Counter.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Counter>>(res);
            List<Counter> resultingAccounts = new List<Counter>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Counter.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Counter>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Identity>>> GetIdentitysAsync(string programAddress = DeityBotProgram.ID, Commitment commitment = Commitment.Confirmed)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Identity.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Identity>>(res);
            List<Identity> resultingAccounts = new List<Identity>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Identity.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Identity>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Agent>> GetAgentAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Agent>(res);
            var resultingAccount = Agent.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Agent>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<ContextAccount>> GetContextAccountAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<ContextAccount>(res);
            var resultingAccount = ContextAccount.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<ContextAccount>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Counter>> GetCounterAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Counter>(res);
            var resultingAccount = Counter.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Counter>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Identity>> GetIdentityAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Identity>(res);
            var resultingAccount = Identity.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Identity>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeAgentAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Agent> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Agent parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Agent.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeContextAccountAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, ContextAccount> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                ContextAccount parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = ContextAccount.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeCounterAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Counter> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Counter parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Counter.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeIdentityAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Identity> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Identity parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Identity.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<DeityBotErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<DeityBotErrorKind>>{};
        }
    }

    namespace Program
    {
        public class CallbackAgentAccounts
        {
            public PublicKey Identity { get; set; }

            public PublicKey User { get; set; }

            public PublicKey Agent { get; set; }

            public PublicKey MintAccount { get; set; }

            public PublicKey AssociatedTokenAccount { get; set; }

            public PublicKey TokenProgram { get; set; } = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
            public PublicKey AssociatedTokenProgram { get; set; } = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
            public PublicKey MinterProgram { get; set; }
        }

        public class InitialiseAgentAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Agent { get; set; }

            public PublicKey LlmContext { get; set; }

            public PublicKey Counter { get; set; }

            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
            public PublicKey Rent { get; set; } = new PublicKey("SysvarRent111111111111111111111111111111111");
            public PublicKey OracleProgram { get; set; } = new PublicKey("LLMrieZMpbJFwN52WgmBNMxYojrpRVYXdC1RCweEbab");
        }

        public class InteractAgentAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Interaction { get; set; }

            public PublicKey Agent { get; set; }

            public PublicKey ContextAccount { get; set; }

            public PublicKey AssociatedTokenAccount { get; set; }

            public PublicKey MintAccount { get; set; }

            public PublicKey OracleProgram { get; set; } = new PublicKey("LLMrieZMpbJFwN52WgmBNMxYojrpRVYXdC1RCweEbab");
            public PublicKey TokenProgram { get; set; } = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
            public PublicKey AssociatedTokenProgram { get; set; } = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
            public PublicKey SystemProgram { get; set; } = new PublicKey("11111111111111111111111111111111");
            public PublicKey MinterProgram { get; set; }
        }

        public static class DeityBotProgram
        {
            public const string ID = "62f9zAUjCN5VFqWF43qSUrW6CvivqhsEjDvCHwQ1SjgR";
            public static Solana.Unity.Rpc.Models.TransactionInstruction CallbackAgent(CallbackAgentAccounts accounts, string response, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Identity, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.User, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Agent, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MintAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.AssociatedTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.MinterProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(4409787423541985137UL, offset);
                offset += 8;
                offset += _data.WriteBorshString(response, offset);
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction InitialiseAgent(InitialiseAgentAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Agent, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.LlmContext, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Counter, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.OracleProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(6762927176650592208UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction InteractAgent(InteractAgentAccounts accounts, byte option, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Interaction, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Agent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ContextAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.AssociatedTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MintAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.OracleProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.MinterProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(6407477965579940893UL, offset);
                offset += 8;
                _data.WriteU8(option, offset);
                offset += 1;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}