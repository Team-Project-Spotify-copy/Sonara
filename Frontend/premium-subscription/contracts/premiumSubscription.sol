// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.20;

contract premiumSubscription {
    address payable public immutable owner;

    enum PlanType { INDIVIDUAL, DUO, FAMILY }

    event SubscriptionPurchased(
        uint256 indexed userId,
        PlanType planType,
        address indexed buyer,
        uint256 amountPaid
    );

    error InsufficientETH();
    error InvalidPlan();
    error TransferFailed();

    constructor() {
        owner = payable(msg.sender);
    }

    function buySubscription(uint256 _userId, PlanType _planType) external payable {
        if (_planType == PlanType.INDIVIDUAL) {
            if (msg.value < 0.00213708 ether) revert InsufficientETH();
        } else if (_planType == PlanType.DUO) {
            if (msg.value < 0.00374391 ether) revert InsufficientETH();
        } else if (_planType == PlanType.FAMILY) {
            if (msg.value < 0.00444092 ether) revert InsufficientETH();
        } else {
            revert InvalidPlan();
        }

        (bool success, ) = owner.call{value: msg.value}("");
        if (!success) revert TransferFailed();

        emit SubscriptionPurchased(_userId, _planType, msg.sender, msg.value);
    }
}