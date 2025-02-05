using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeityBot;
using DeityBot.Accounts;
using Model;
using Newtonsoft.Json;
using Smartobjectdeity.Accounts;
using Smartobjectlocation.Accounts;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utils.Injection;
using View;
using World;

namespace Connectors
{
    public class ChatNode
    {
        public string reply;
        public string[] options;
    }
    
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SmartObjectDeityConnector : BaseComponentConnector<SmartObjectDeity>
    {
        [Inject] private DialogInteractionStateModel _dialogInteractionState;
        
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
            var streamingClient = await GetStreamingClient();
            PublicKey.TryFindProgramAddress(new[]
            {
                Encoding.UTF8.GetBytes("hots_agent")
                
            }, AgentProgramId, out var agentAddress, out _);

            // var agentAddress = "CjgncsnDm7YLtVtKxRMrhPzkmFshucvb61QXDzSMjsQJ";
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
                AccountMeta.ReadOnly(agentAddress, false),
                AccountMeta.ReadOnly(ContextAccount, false),
                AccountMeta.ReadOnly(OracleProgramId, false),
            };

            if (_sub != null)
                await streamingClient.UnsubscribeAsync(_sub);

            _sub = await streamingClient.SubscribeLogInfoAsync(InteractionAccount,
                (_, value) => { ProcessLogs(value.Value.Logs); }, Commitment.Processed);
        }

        public async Task<bool> Interact(int index, PublicKey systemAddress,
            AccountMeta[] extraAccounts)
        {
            Dimmer.Visible = true;
            
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

        private void ProcessLogs(string[] value)
        {
            foreach (var log in value)
            {
                if (log.Contains("Agent Reply: "))
                {
                    Debug.Log(log);
                    var startIndex = log.IndexOf("{");
                    var lastIndexOf = log.LastIndexOf("}");
                    var parsed  = log.Substring(startIndex, lastIndexOf - startIndex + 1);
                    
                    parsed = parsed.Replace("\\n", "");
                    parsed = parsed.Replace("\\", "");
                    
                    var data = JsonConvert.DeserializeObject<ChatNode>(parsed);
                    
                    Debug.Log(parsed);

                    Dimmer.Visible = false;
                    _dialogInteractionState.SetCurrentChat(data);
                    
                    break;
                }
            }
        }
    }
}