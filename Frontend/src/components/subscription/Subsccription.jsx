import { useState, useContext, useEffect, useCallback } from "react";
import axios from "axios";
import image from "../../assets/images/subscription-hd-bg.png";
import { buySubscription } from "../../utilites/blockchainUtils";
import { AccountContext } from "../../contexts/account.store";
import "../../css/SubscriptionPage.css";

export const PLAN_TYPE = Object.freeze({
  INDIVIDUAL: 0,
  DUO: 1,
  FAMILY: 2,
  Free: 3,
});

const PLAN_NAME_BY_TYPE = {
  [PLAN_TYPE.INDIVIDUAL]: "Individual",
  [PLAN_TYPE.DUO]: "Duo",
  [PLAN_TYPE.FAMILY]: "Family",
  [PLAN_TYPE.Free]: "Free",
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

export default function Subscription() {
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
      planType: PLAN_TYPE.Free,
      Name: "Free",
      Price: 0,
      SubscriptionDescription: "0,00 US$ per month after.",
      Features: ["1 account", "Forever"],
    },
    {
      planType: PLAN_TYPE.INDIVIDUAL,
      Name: "Individual",
      Price: 3.99,
      SubscriptionDescription: "3,99 US$ per month after.",
      Features: ["1 Premium account", "Cancel anytime"],
    },
    {
      planType: PLAN_TYPE.DUO,
      Name: "Duo",
      Price: 6.99,
      SubscriptionDescription: "5,99 US$ per month after.",
      Features: ["2 Premium accounts", "Cancel anytime"],
    },
    {
      planType: PLAN_TYPE.FAMILY,
      Name: "Family",
      Price: 9.99,
      SubscriptionDescription: "7,99 US$ per month after.",
      Features: [
        "Up to 6 Premium accounts",
        "Parental controls for the plan manager",
        "Cancel anytime",
      ],
    },
  ];

  return (
    <div className="subscription-container">
      <div
        className="subscription-header"
        style={{ backgroundImage: `url(${image})`}}
      >
        <h1 className="subscription-header-text">
          Listen to music without limits.
        </h1>
        <h2 className="subscription-header-sub-text">
          Subscribe to a personalized Premium plan and try it free for the first
          month.
        </h2>

        <div className="subscription-header-div-button">
          <button className="subscription-header-button">
            View all subscriptions{" "}
          </button>
        </div>
      </div>

      <div className="subscription-card-wrapper">
        <h1 className="subscription-title">
          Affordable plans for any situation
        </h1>
        <h2 className="subscription-sub-title">
          All Premium plans include: ad-free music listening, download to listen
          offline, play songs <br /> in any order, high audio quality, listen
          with friends in real time, organise listening queue
        </h2>

        {status && (
          <p className={`subscription-status subscription-status--${status}`}>
            {statusMessage}
          </p>
        )}

        <div className="plans-grid">
          {subscriptionPlans.map((card, index) => {
            const isThisPlanBusy = isBusy && activePlan === card.planType;
            const isCurrentPlan = currentSubscription?.plan?.name === card.Name;
            const planNames = {
              0: "Individual",
              1: "Duo",
              2: "Family",
              3: "Free",
            };

            let buttonLabel = `Get Premium ${planNames[card.planType]}`;
            if (isThisPlanBusy) buttonLabel = "Processing...";
            else if (isLoadingSubscription) buttonLabel = "Loading...";
            else if (isCurrentPlan) buttonLabel = "Your current plan";

            const isDisabled = isBusy || isLoadingSubscription || isCurrentPlan;

            return (
              <div
                key={index}
                className={`plan-card ${planNames[card.planType]}`}
              >
                <div>
                  <p className="plan-name">{card.Name}</p>
                </div>

                <div>
                  <p className="plan-price">
                    ${card.Price}
                    <span className="plan-price-suffix"> /month</span>
                  </p>

                  <hr className="plan-divider" />

                  <div className="plan-feature-container">
                    {card.Features.map((feature, featureIndex) => (
                      <p key={featureIndex} className="plan-feature-item">
                        <span className="dot">•</span>
                        <span className="feature-text">{feature}</span>
                      </p>
                    ))}
                  </div>
                </div>

                <button
                  className={"plan-button"}
                  disabled={isDisabled}
                  onClick={() => handleBuySubscription(card.planType)}
                >
                  {buttonLabel}
                </button>

                <p className="plan-description">
                  {card.SubscriptionDescription}
                </p>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
