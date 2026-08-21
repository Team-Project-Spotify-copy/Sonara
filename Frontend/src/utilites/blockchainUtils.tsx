import { BrowserProvider, Contract, ethers, Signer } from "ethers";
import contractArtifact from "../../premium-subscription/artifacts/contracts/premiumSubscription.sol/premiumSubscription.json";

const contractAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";
const abi = contractArtifact.abi;

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

export const buySubscription = async (userId: number, planType: PlanType): Promise<boolean> => {
  return (
    (await withContract(async (contract) => {
      const price = PLAN_PRICES[planType];
      
      const tx = await contract.buySubscription(userId, planType, {
        value: ethers.parseEther(price),
      });

      await tx.wait();
      return true;
    })) ?? false
  );
};