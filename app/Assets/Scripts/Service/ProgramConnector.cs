using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Settlement;
using Settlement.Program;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using View.UI;
using World.Program;

// ReSharper disable InconsistentNaming (this is for args passing without renaming etc to match rust)

namespace Service
{
    [Singleton]
    public class ProgramConnector : InjectableObject<ProgramConnector>
    {
        [Inject] private SettlementModel _settlement;

        //this comes from program deployment
        private const string WorldPda = "GvMv6N5UF8ctteapSXMJUh2GXmXb4a7hRHWNmi69PTA8";
        private const string SettlementProgramAddress = "B2h45ZJwpiuD9jBY7Dfjky7AmEzdzGsty4qWQxjX9ycv";
        private const int WorldIndex = 1318;

        private SettlementClient _client;

        private SettlementClient Settlement =>
            _client ??= new SettlementClient(Web3.Rpc, Web3.WsRpc, new PublicKey(SettlementProgram.ID));

        private string _dataAddress;
        private string _entityPda;
        private long _offset;

        private string EntityPda =>
            _entityPda ??= Pda.FindEntityPda(WorldIndex, 0, ExtraSeed);

        private static string ExtraSeed => Web3.Account.PublicKey.Key[..20];


        public async Task EnsureBalance()
        {
            var requestResult = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            Debug.Log($"{Web3.Account.PublicKey} {requestResult.Result} ");
            
            if (requestResult.Result.Value < 50000000)
            {
                await Airdrop();
            }

        }
        
        public async Task Airdrop()
        {
            RequestResult<string> airdropResult = null;
            airdropResult = await Web3.Rpc.RequestAirdropAsync(Web3.Account.PublicKey, 100000000);
            var txResult = await Web3.Rpc.ConfirmTransaction(airdropResult.Result, Commitment.Confirmed);
            var balanceResult = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            Debug.Log($"{Web3.Account.PublicKey} \nairdropResult.Result: {airdropResult.Result}, \ntxResult{txResult} \n balanceResult:{balanceResult} ");
        }

        public async Task<bool> ReloadData()
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");
            var walletBase = Web3.Wallet;

            if (_dataAddress == null)
            {
                //add player entity if doesnt exist

                var playerEntityState = await Web3.Rpc.GetAccountInfoAsync(EntityPda);
                if (playerEntityState.Result.Value == null)
                {
                    var tx = new Transaction
                    {
                        FeePayer = Web3.Account,
                        Instructions = new List<TransactionInstruction>
                        {
                            WorldProgram.AddEntity(new AddEntityAccounts()
                            {
                                Payer = Web3.Account.PublicKey,
                                World = new(WorldPda),
                                Entity = new(EntityPda),
                                SystemProgram = SystemProgram.ProgramIdKey
                            }, ExtraSeed)
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };


                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                var dataAddress = Pda.FindComponentPda(new(EntityPda), new(SettlementProgramAddress));

                var componentDataState = await Web3.Rpc.GetAccountInfoAsync(EntityPda);
                if (componentDataState.Result.Value == null)
                {
                    var tx = new Transaction
                    {
                        FeePayer = Web3.Account,
                        Instructions = new List<TransactionInstruction>
                        {
                            WorldProgram.InitializeComponent(new InitializeComponentAccounts()
                            {
                                Payer = Web3.Account,
                                Entity = new(EntityPda),
                                Data = dataAddress,
                                ComponentProgram = new(SettlementProgramAddress),
                                SystemProgram = SystemProgram.ProgramIdKey
                            })
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };

                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                _dataAddress = dataAddress;
            }

            var rawData = await Settlement.GetSettlementAsync(_dataAddress, Commitment.Processed);

            if (rawData.ParsedResult == null)
                return false;

            Debug.Log($"Data:\n {JsonConvert.SerializeObject(rawData.ParsedResult)}");
            _settlement.Set(rawData.ParsedResult);

            return true;
        }

        public async Task<bool> PlaceBuilding(byte x, byte y, byte type, int worker_index)
        {
            return await ApplySystem(new PublicKey("AoKVKur4mczZtuzeMQwydkMe6ZSrJGxTWqZU6grPnd9c"),
                new { x, y, config_index = type, worker_index });
        }

        public async Task<bool> Wait(int time)
        {
            return await ApplySystem(new PublicKey("5LiZ8jP6fqAWT5V6B3C13H9VCwiQoqdyPwUYzWDfMUSy"),
                new { time });
        }

        public async Task<bool> AssignLabour(int worker_index, int building_index)
        {
            return await ApplySystem(new PublicKey("F7m12a5YbScFwNPrKXwg4ua6Z9e7R1ZqXvXigoUfFDMq"),
                new { worker_index, building_index });
        }


        public async Task<bool> Repair(int index)
        {
            return await ApplySystem(new PublicKey("4MA6KhwEUsLbZJqJK9rqwVjdZgdxy7vbebuD2MeLKm5j"), new { index });
        }

        public async Task<bool> Upgrade(int index)
        {
            return await ApplySystem(new PublicKey("J3evfUppPdgjTzWhhAhuhKBVM23UU8iCU9j9r7sTHCTB"), new { index });
        }

        public async Task<bool> ClaimTime()
        {
            return await ApplySystem(new PublicKey("HFx2weMbr8CrAEAPfPtgw9zzgHgUFzSz7qiTyhTHGSF"), new { });
        }

        public async Task<bool> Research(int research_type)
        {
            return await ApplySystem(new PublicKey("GnVJxqk8dExpXhVidSEFNQcjTY1sCAYWcwM1GGVKKVHb"),
                new { research_type });
        }

        public async Task<bool> Sacrifice(int index)
        {
            return await ApplySystem(new PublicKey("4Cvjz6qrVakbSg3dqBMA8vv8XL8KD3UCTbRVM8g8WkoW"),
                new { index });
        }

        public async Task<bool> Reset()
        {
            return await ApplySystem(new PublicKey("J2HTjpKDf317Q7Pg9kUVFDregE2Ld34P61M5m4XnVSh2"),
                new {  });
        }

        private async Task<bool> ApplySystem(PublicKey system, object args)
        {
            Dimmer.Visible = true;
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    WorldProgram.ApplySystem(
                        system,
                        new[]
                        {
                            new WorldProgram.EntityType(new(EntityPda),
                                new[] { new PublicKey(SettlementProgramAddress) })
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Processed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log($"ApplySystem: {result.Result}");

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Processed);

            Dimmer.Visible = false;
            return result.WasSuccessful;
        }

        public async Task SyncTime()
        {
            var slot = await Web3.Rpc.GetSlotAsync(Commitment.Processed);
            var nodeTimestamp = await Web3.Rpc.GetBlockTimeAsync(slot.Result);
            _offset = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (long)nodeTimestamp.Result;

            Debug.Log(_offset);
        }

        public long GetNodeTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _offset;
        }
    }
}