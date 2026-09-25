import { BrowserProvider, Contract, ethers, Signer } from "ethers";
import contractArtifact from "../../premium-subscription/artifacts/contracts/premiumSubscription.sol/premiumSubscription.json";

const contractAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";
const abi = [
    {
      "inputs": [],
      "stateMutability": "nonpayable",
      "type": "constructor"
    },
    {
      "inputs": [],
      "name": "InsufficientETH",
      "type": "error"
    },
    {
      "inputs": [],
      "name": "InvalidPlan",
      "type": "error"
    },
    {
      "inputs": [],
      "name": "TransferFailed",
      "type": "error"
    },
    {
      "anonymous": false,
      "inputs": [
        {
          "indexed": false,
          "internalType": "string",
          "name": "userId",
          "type": "string"
        },
        {
          "indexed": false,
          "internalType": "enum premiumSubscription.PlanType",
          "name": "planType",
          "type": "uint8"
        },
        {
          "indexed": true,
          "internalType": "address",
          "name": "buyer",
          "type": "address"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "amountPaid",
          "type": "uint256"
        }
      ],
      "name": "SubscriptionPurchased",
      "type": "event"
    },
    {
      "inputs": [
        {
          "internalType": "string",
          "name": "_userId",
          "type": "string"
        },
        {
          "internalType": "enum premiumSubscription.PlanType",
          "name": "_planType",
          "type": "uint8"
        }
      ],
      "name": "buySubscription",
      "outputs": [],
      "stateMutability": "payable",
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

// ВАЖЛИВО: це має відповідати мережі, яку слухає бекенд (Ethereum:Url).
// Для локальної Hardhat-ноди chain ID зазвичай 31337.
// Якщо MetaMask підключений до іншої мережі, транзакція піде туди, де
// вашого контракту й BlockchainListenerService немає — бекенд ніколи
// не побачить подію, хоча у фронті все виглядатиме як успіх.
const EXPECTED_CHAIN_ID = 31337n;

declare global {
  interface Window {
    ethereum?: any;
  }
}

const getProvider = (): BrowserProvider | null => {
  if (!window.ethereum) {
    alert("Please install MetaMask!");
    return null;
  }
  return new ethers.BrowserProvider(window.ethereum);
};

export const getSigner = async (): Promise<Signer | null> => {
  try {
    const provider = getProvider();
    if (!provider) return null;

    await window.ethereum.request({ method: "eth_requestAccounts" });

    // ПЕРЕВІРКА МЕРЕЖІ: якщо не спіймати це тут, транзакція просто піде
    // в порожнечу і фронт мовчки чекатиме TIMEOUT замість зрозумілої помилки.
    const network = await provider.getNetwork();
    if (network.chainId !== EXPECTED_CHAIN_ID) {
      alert(
        `Неправильна мережа в MetaMask (chainId=${network.chainId}). ` +
          `Перемкніться на локальну мережу розробки (chainId=${EXPECTED_CHAIN_ID}).`,
      );
      return null;
    }

    return await provider.getSigner();
  } catch (error) {
    console.error("User denied account access or network check failed:", error);
    return null;
  }
};

export const getContract = async (): Promise<Contract | null> => {
  const signer = await getSigner();
  if (!signer) return null;

  const contract = new Contract(contractAddress, abi, signer);

  // Швидка перевірка, що за цією адресою в поточній мережі дійсно є код
  // контракту. Якщо ноду передеплоїли/перезапустили, адреса могла
  // "спорожніти" або належати іншому контракту — краще впасти тут з
  // явною помилкою, ніж отримати незрозумілий revert пізніше.
  const provider = signer.provider;
  if (provider) {
    const code = await provider.getCode(contractAddress);
    if (code === "0x") {
      console.error(
        `За адресою ${contractAddress} немає задеплоєного контракту в поточній мережі.`,
      );
      return null;
    }
  }

  return contract;
};

const withContract = async (
  action: (contract: Contract) => any,
): Promise<any | void> => {
  const contract = await getContract();
  if (!contract) return;
  try {
    return await action(contract);
  } catch (error) {
    console.error("Contract interaction error:", error);
  }
};

export enum PlanType {
  INDIVIDUAL = 0,
  DUO = 1,
  FAMILY = 2,
}

const PLAN_PRICES: Record<PlanType, string> = {
  [PlanType.INDIVIDUAL]: "0.00213708",
  [PlanType.DUO]: "0.00374391",
  [PlanType.FAMILY]: "0.00444092",
};

export const buySubscription = async (
  userId: string,
  planType: PlanType,
): Promise<{ success: boolean; txHash?: string }> => {
  return (
    (await withContract(async (contract) => {
      const price = PLAN_PRICES[planType];
      const tx = await contract.buySubscription(userId, planType, {
        value: ethers.parseEther(price),
      });
      const receipt = await tx.wait();
      return { success: receipt.status === 1, txHash: tx.hash };
    })) ?? { success: false }
  );
};