import { BrowserProvider, Contract, ethers, Signer } from "ethers";

const contractAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";
const abi = [
    {
      "inputs": [],
      "stateMutability": "nonpayable",
      "type": "constructor"
    },
    {
      "anonymous": false,
      "inputs": [
        {
          "indexed": true,
          "internalType": "int256",
          "name": "id",
          "type": "int256"
        },
        {
          "indexed": false,
          "internalType": "address",
          "name": "buyer",
          "type": "address"
        }
      ],
      "name": "PremiumBought",
      "type": "event"
    },
    {
      "inputs": [
        {
          "internalType": "int256",
          "name": "_id",
          "type": "int256"
        }
      ],
      "name": "buyPremium",
      "outputs": [
        {
          "internalType": "bool",
          "name": "",
          "type": "bool"
        }
      ],
      "stateMutability": "payable",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "int256",
          "name": "",
          "type": "int256"
        }
      ],
      "name": "isPremium",
      "outputs": [
        {
          "internalType": "bool",
          "name": "",
          "type": "bool"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    },
    {
      "inputs": [],
      "name": "owner",
      "outputs": [
        {
          "internalType": "address payable",
          "name": "",
          "type": "address"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    }
  ];

declare global { interface Window { ethereum?: any; } }

const getProvider = (): BrowserProvider | null => {
  if (!window.ethereum) { alert("Please install MetaMask!"); return null; }
  return new ethers.BrowserProvider(window.ethereum);
};

export const getSigner = async (): Promise<Signer | null> => {
  try {
    const provider = getProvider();
    if (!provider) return null;
    await window.ethereum.request({ method: "eth_requestAccounts" });
    return await provider.getSigner();
  } catch (error) {
    console.error("User denied account access:", error);
    return null;
  }
};

export const getContract = async (): Promise<Contract | null> => {
  const signer = await getSigner();
  return signer ? new Contract(contractAddress, abi, signer) : null;
};

const withContract = async (action: (contract: Contract) => any): Promise<any | void> => {
  const contract = await getContract();
  if (!contract) return;
  try { return await action(contract); } catch (error) { console.error("Contract interaction error:", error); }
};

export const buyPremium = async (id: number): Promise<boolean> => {
  return await withContract(async (contract) => {
    const tx = await contract.buyPremium(id, { value: ethers.parseEther("1") });
    await tx.wait();
    return true;
  }) ?? false;
};

export const getIsPremiumStatus = async (id: number): Promise<boolean> => {
  return await withContract(async (contract) => {
    return await contract.isPremium(id);
  }) ?? false;
};