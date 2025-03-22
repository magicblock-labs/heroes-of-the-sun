using System;
using System.Threading.Tasks;
using Model;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Utils;
using Utils.Injection;


namespace Connectors
{
    [Singleton]
    public class TokenConnector : InjectableObject
    {
        [Inject] private TokenModel _model;

        private const string TokenMintPda = "Fn7ndp5EocCfzDkFMdWUZj5B55AoM7nA5o5cXSUbtDrn";
        private const string TokenMinterProgramID = "4ZxRnucEWC62kVktmx27cz9d1PzWWNgiZLT5VWFLbfB2";

        private string _ata;

        private string AssociatedTokenAccount => _ata ??=
            AssociatedTokenAccountProgram
                .DeriveAssociatedTokenAccount(Web3.Account, new PublicKey(TokenMintPda));


        public async Task LoadData()
        {
            var accountInfo = await Web3.Wallet.ActiveRpcClient.GetAccountInfoAsync(AssociatedTokenAccount);
            if (accountInfo.Result.Value != null)
                _model.Set(AccountLayout.DeserializeAccountLayout(accountInfo.Result.Value.Data[0]));
        }

        public async Task Subscribe(Action<SubscriptionState, ResponseValue<AccountInfo>, AccountInfo> callback)
        {
            await Web3.Wallet.ActiveStreamingRpcClient.SubscribeAccountInfoAsync(AssociatedTokenAccount,
                (s, accountInfo) => { _model.Set(AccountLayout.DeserializeAccountLayout(accountInfo.Value.Data[0])); },
                Commitment.Processed);
        }


        public AccountMeta[] GetMintExtraAccounts()
        {
            var authority = Web3Utils.SessionToken == null
                ? Web3.Wallet.Account
                : Web3Utils.SessionWallet.Account;
            
            return new[]
            {
                AccountMeta.Writable(authority, true),
                AccountMeta.Writable(new PublicKey(AssociatedTokenAccount), false),
                AccountMeta.Writable(new PublicKey(TokenMintPda), false),
                AccountMeta.ReadOnly(new PublicKey(TokenMinterProgramID), false),
                AccountMeta.ReadOnly(TokenProgram.ProgramIdKey, false),
                AccountMeta.ReadOnly(AssociatedTokenAccountProgram.ProgramIdKey, false),
                AccountMeta.ReadOnly(SystemProgram.ProgramIdKey, false)
            };
        }

        public AccountMeta[] GetBurnExtraAccounts()
        {
            
            var authority = Web3Utils.SessionToken == null
                ? Web3.Wallet.Account
                : Web3Utils.SessionWallet.Account;

            
            return new[]
            {
                AccountMeta.Writable(authority, true),
                AccountMeta.Writable(new PublicKey(AssociatedTokenAccount), false),
                AccountMeta.Writable(new PublicKey(TokenMintPda), false),
                AccountMeta.ReadOnly(new PublicKey(TokenMinterProgramID), false),
                AccountMeta.ReadOnly(TokenProgram.ProgramIdKey, false),
                AccountMeta.ReadOnly(AssociatedTokenAccountProgram.ProgramIdKey, false)
            };
        }
    }
}