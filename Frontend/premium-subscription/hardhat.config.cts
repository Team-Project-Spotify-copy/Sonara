import dotenv = require("dotenv");
dotenv.config();

require("@nomicfoundation/hardhat-toolbox");

/** @type import('hardhat/config').HardhatUserConfig */
const config = {
  solidity: {
    version: "0.8.20",
    settings: {
      optimizer: {
        enabled: true,
        runs: 200,
      },
    },
  },

  networks: {
    localhost: {
      url: "http://127.0.0.1:8545",
      accounts: [
        "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80"
      ],
    },
    sepolia: { 
      url: process.env.SEPOLIA_URL || "",  
      accounts: process.env.PRIVATE_KEY ?[process.env.PRIVATE_KEY] : [],
    },
  },
};

module.exports = config;