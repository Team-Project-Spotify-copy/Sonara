import React, { useState, useContext, useEffect, useCallback } from "react";
import axios from "axios";
import image from "../assets/images/register-bg.png";
import { buySubscription } from "../utilites/blockchainUtils";
import { AccountContext } from "../contexts/account.store";
import { useNavigate, Link } from "react-router-dom";
import "../css/SubscriptionPage.css";

export const PLAN_TYPE = Object.freeze({
  INDIVIDUAL: 0,
  DUO: 1,
  FAMILY: 2,
});

const PLAN_NAME_BY_TYPE = {
  [PLAN_TYPE.INDIVIDUAL]: "Individual",
  [PLAN_TYPE.DUO]: "Duo",
  [PLAN_TYPE.FAMILY]: "Family",
};

const STATUS = {
  PENDING: "pending",
  CONFIRMING: "confirming",
  CONFIRMED: "confirmed",
  ERROR: "error",
  TIMEOUT: "timeout",
};

const api = import.meta.env.VITE_API;

const fetchMySubscription = async (accessToken) => {
  try {
    const headers = accessToken
      ? { Authorization: `Bearer ${accessToken}` }
      : {};

    const res = await axios.get(`${api}/subscriptions/me`, { headers });
    return res.data;
  } catch (err) {
    if (err.response?.status === 404) return null;
    console.error("Failed to fetch current subscription:", err);
    return null;
  }
};

const waitForExpectedPlan = async (
  accessToken,
  expectedPlanName,
  timeoutMs = 20000,
  intervalMs = 1500,
) => {
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    const sub = await fetchMySubscription(accessToken);
    if (sub?.plan?.name === expectedPlanName) {
      return sub; 
    }
    await new Promise((r) => setTimeout(r, intervalMs));
  }
  return null; 
};

export default function SubscriptionPage() {
  const navigate = useNavigate();
  const { userId, accessToken } = useContext(AccountContext);

  const [status, setStatus] = useState(null);
  const [activePlan, setActivePlan] = useState(null);
  const [currentSubscription, setCurrentSubscription] = useState(null);
  const [isLoadingSubscription, setIsLoadingSubscription] = useState(true);

  const loadCurrentSubscription = useCallback(async () => {
    setIsLoadingSubscription(true);
    const sub = await fetchMySubscription(accessToken);
    setCurrentSubscription(sub);
    setIsLoadingSubscription(false);
  }, [accessToken]);

  useEffect(() => {
    if (!userId || !accessToken) {
      setIsLoadingSubscription(false);
      return;
    }
    loadCurrentSubscription();
  }, [userId, accessToken, loadCurrentSubscription]);

  const handleBuySubscription = async (planType) => {
    if (!userId) {
      console.error("User is not authenticated");
      return;
    }
    if (status === STATUS.PENDING || status === STATUS.CONFIRMING) return;

    setActivePlan(planType);
    setStatus(STATUS.PENDING);

    const result = await buySubscription(userId, planType);

    if (!result.success) {
      setStatus(STATUS.ERROR);
      return;
    }

    setStatus(STATUS.CONFIRMING);

    const expectedPlanName = PLAN_NAME_BY_TYPE[planType];
    const updatedSub = await waitForExpectedPlan(accessToken, expectedPlanName);

    if (updatedSub) {
      setCurrentSubscription(updatedSub); 
      setStatus(STATUS.CONFIRMED);
    } else {
      setStatus(STATUS.TIMEOUT);
    }
  };

  const statusMessage = {
    [STATUS.PENDING]: "Підтвердіть транзакцію в MetaMask...",
    [STATUS.CONFIRMING]: "Транзакція підтверджена, активуємо підписку...",
    [STATUS.CONFIRMED]: "Підписку активовано! 🎉",
    [STATUS.ERROR]: "Помилка транзакції. Спробуйте ще раз.",
    [STATUS.TIMEOUT]:
      "Транзакція пройшла, але активація затримується. Оновіть сторінку за хвилину.",
  }[status];

  const isBusy = status === STATUS.PENDING || status === STATUS.CONFIRMING;

  const subscriptionPlans = [
    {
      planType: PLAN_TYPE.INDIVIDUAL,
      Name: "Individual",
      Accounts: "1 account",
      Price: 3.99,
      SubscriptionDescription:
        "Ad-free music listening, offline playback, and unlimited skips for 1 individual account.",
      Features: [
        "1 Premium account",
        "Cancel dynamic subscription anytime",
        "Ad-free music listening",
        "Download to listen offline",
        "High quality audio streaming",
        "Unlimited track skips",
      ],
    },
    {
      planType: PLAN_TYPE.DUO,
      Name: "Duo",
      Accounts: "2 accounts",
      Price: 6.99,
      SubscriptionDescription:
        "2 Premium accounts for couples or friends living together with shared playlist features.",
      Features: [
        "2 Premium accounts",
        "Cancel dynamic subscription anytime",
        "Ad-free music listening",
        "Download to listen offline",
        "Shared Duo Mix playlist",
        "Unlimited track skips",
      ],
    },
    {
      planType: PLAN_TYPE.FAMILY,
      Name: "Family",
      Accounts: "Up to 6 accounts",
      Price: 9.99,
      SubscriptionDescription:
        "Up to 6 Premium accounts for family members with invite management system.",
      Features: [
        "Up to 6 Premium accounts",
        "Invite system (up to 6 friends/family)",
        "Ad-free music listening",
        "Download to listen offline",
        "Explicit music blocking filter",
        "Unlimited track skips",
      ],
    },
  ];

  return (
    <div className="subscription-container">
      <div>
        <Link className="nav-link-circle left" to="/">
          <span>Home</span>
        </Link>

        <Link className="nav-link-circle right" to="/account/">
          Profile
        </Link>
      </div>
      <div
        className="subscription-card-wrapper"
        style={{ backgroundImage: `url(${image})` }}
      >
        <h1 className="subscription-title">Choose Your Plan</h1>

        {status && (
          <p className={`subscription-status subscription-status--${status}`}>
            {statusMessage}
          </p>
        )}

        <div className="plans-grid">
          {subscriptionPlans.map((card, index) => {
            const isThisPlanBusy = isBusy && activePlan === card.planType;
            const isCurrentPlan = currentSubscription?.plan?.name === card.Name;

            let buttonLabel = "Choose Plan";
            if (isThisPlanBusy) buttonLabel = "Обробка...";
            else if (isLoadingSubscription) buttonLabel = "Завантаження...";
            else if (isCurrentPlan) buttonLabel = "Ваш поточний план";

            const isDisabled = isBusy || isLoadingSubscription || isCurrentPlan;

            return (
              <div
                key={index}
                className={`plan-card${isCurrentPlan ? " plan-card--current" : ""}`}
              >
                <div>
                  <p className="plan-name">{card.Name}</p>
                  <p className="plan-accounts">{card.Accounts}</p>
                </div>

                <div>
                  <p className="plan-price">
                    ${card.Price}
                    <span className="plan-price-suffix"> /month</span>
                  </p>

                  <p className="plan-billing">Billed monthly</p>

                  <hr className="plan-divider" />

                  <p className="plan-description">
                    {card.SubscriptionDescription}
                  </p>
                </div>

                <button
                  className={`plan-button${isCurrentPlan ? " plan-button--current" : ""}`}
                  disabled={isDisabled}
                  onClick={() => handleBuySubscription(card.planType)}
                >
                  {buttonLabel}
                </button>

                <div>
                  {card.Features.map((feature, featureIndex) => (
                    <p key={featureIndex} className="plan-feature-item">
                      {feature}
                    </p>
                  ))}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
