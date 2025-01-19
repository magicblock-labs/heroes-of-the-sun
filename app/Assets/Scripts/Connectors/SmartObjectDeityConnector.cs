using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeityBot;
using DeityBot.Accounts;
using Smartobjectdeity.Accounts;
using Smartobjectlocation.Accounts;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using World;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SmartObjectDeityConnector : BaseComponentConnector<SmartObjectDeity>
    {
        private bool _initialised;

        public PublicKey InteractionAccount;
        public PublicKey ContextAccount;

        public PublicKey AgentProgramId = new("62f9zAUjCN5VFqWF43qSUrW6CvivqhsEjDvCHwQ1SjgR");
        public PublicKey OracleProgramId = new("LLMrieZMpbJFwN52WgmBNMxYojrpRVYXdC1RCweEbab");
        private AccountMeta[] _interactionAccounts;

        protected override SmartObjectDeity DeserialiseBytes(byte[] value)
        {
            return SmartObjectDeity.Deserialize(value);
        }

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("9RfzWgEBYQAM64a46V3dGRPKYsVY8a7YvZszWPMxvBfk");
        }

        public async Task Initialize()
        {
            // PublicKey.TryFindProgramAddress(new[]
            // {
            //     Encoding.UTF8.GetBytes("agent"),
            //     Web3.Account.PublicKey.KeyBytes,
            //     
            // }, AgentProgramId, out var agentAddress, out _);

            var agentAddress = "CjgncsnDm7YLtVtKxRMrhPzkmFshucvb61QXDzSMjsQJ";
            var agent = await RpcClient.GetAccountInfoAsync(agentAddress);

            if (agent.WasSuccessful)
            {
                var agentAccount = Agent.Deserialize(Convert.FromBase64String(agent.Result.Value.Data[0]));
                ContextAccount = agentAccount.Context;

                PublicKey.TryFindProgramAddress(new[]
                {
                    Encoding.UTF8.GetBytes("interaction"), 
                    Web3.Account.PublicKey.KeyBytes,
                    agentAccount.Context.KeyBytes
                }, OracleProgramId, out InteractionAccount, out _);
            }

            _interactionAccounts = new[]
            {
                AccountMeta.ReadOnly(AgentProgramId, false),
                AccountMeta.Writable(InteractionAccount, false),
                AccountMeta.ReadOnly(new PublicKey(agentAddress), false),
                AccountMeta.ReadOnly(ContextAccount, false),
                AccountMeta.ReadOnly(OracleProgramId, false),
            };
        }

        public async Task<bool> Interact(int index, PublicKey systemAddress,
            AccountMeta[] extraAccounts)
        {
            if (_interactionAccounts == null)
                await Initialize();


            if (_interactionAccounts == null)
            {
                Debug.LogError($"Could not find any interaction accounts.");
                return false;
            }

            return await ApplySystem(systemAddress, new { index }, null, false,
                extraAccounts.Concat(_interactionAccounts).ToArray());
        }
    }
}