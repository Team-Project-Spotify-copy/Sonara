import hre from "hardhat";

async function main() {
  const ContractFactory = await hre.ethers.getContractFactory("premiumSubscription");
  console.log("Deploying contract...");

  const contract = await ContractFactory.deploy();
  await contract.waitForDeployment();

  console.log("Contract deployed to:", await contract.getAddress());
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1; 
});