// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.20;

contract premiumSubscription {
    address payable public owner;
    mapping(int256 => bool) public isPremium;

    event PremiumBought(int256 indexed id, address buyer);

    constructor() {
        owner = payable(msg.sender); 
    }

    function buyPremium(int256 _id) payable public returns(bool) {
        require(!isPremium[_id], "This user already has a premium subscribe");
        require(msg.value >= 1 ether, "Need to send at least 1 ETH");

        (bool success, ) = owner.call{value: msg.value}("");
        require(success, "Transfer failed");

        isPremium[_id] = true;

        emit PremiumBought(_id, msg.sender);
        return true;
    }
}