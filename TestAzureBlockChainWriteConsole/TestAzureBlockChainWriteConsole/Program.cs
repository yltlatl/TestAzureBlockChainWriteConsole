using System;
using System.Threading.Tasks;
using System.Threading;


namespace TestAzureBlockChainWriteConsole
{
    class Program
    {
        static void Main (string[] args)
        {
            var result = DeployContract().GetAwaiter().GetResult();

            Console.WriteLine(result);

            Console.ReadLine();
        }

        static async Task<bool> DeployContract()
        {
            //trying this with the genesis block account instead
            //var senderAddress = "0xF0FC30D327447bDa40F7081Dd3855b6901B37447";
            //var password = "LD1sc0v3ry!";
            var senderAddress = "0xfab5d94a10d4342026ab2abbfb928377b3acf4f9";
            var password = "q9HDK8tDdBV8";
            var abi = @"[{""constant"":false,""inputs"":[{""name"":""val"",""type"":""int256""}],""name"":""multiply"",""outputs"":[{""name"":""d"",""type"":""int256""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""name"":""multiplier"",""type"":""int256""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""constructor""}]";
            var byteCode = "6060604052341561000f57600080fd5b6040516020806100d0833981016040528080516000555050609b806100356000396000f300606060405260043610603e5763ffffffff7c01000000000000000000000000000000000000000000000000000000006000350416631df4f14481146043575b600080fd5b3415604d57600080fd5b60566004356068565b60405190815260200160405180910390f35b60005402905600a165627a7a723058204b555172cdfdd4542892daabc7d7da51eb044d47f5bffaac484db165be55bfa80029";

            var multiplier = 7;

            var web3 = new Nethereum.Web3.Web3("http://eth002au3nds.eastus2.cloudapp.azure.com:8545");
            var unlockAccountResult = await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, 120);

            if (!unlockAccountResult)
            {
                throw new Exception("Unable to unlock account.");
            }

            var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, new Nethereum.Hex.HexTypes.HexBigInteger("0x2710"), multiplier);

            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            while (receipt == null)
            {
                Thread.Sleep(5000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            var contractAddress = receipt.ContractAddress;

            var contract = web3.Eth.GetContract(abi, contractAddress);

            var multiplyFunction = contract.GetFunction("multiply");

            var result = await multiplyFunction.CallAsync<int>(7);

            if (result != 49)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
