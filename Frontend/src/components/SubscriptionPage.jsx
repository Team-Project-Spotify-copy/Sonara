import React from "react";
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

export default function SubscriptionPage() {
  const navigate = useNavigate();

  const { userId } = React.useContext(AccountContext);

  const handleBuySubscription = (planType) => {
    if (!userId) {
      console.error("User is not authenticated");
      return;
    }
    buySubscription(userId, planType);
  };

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

        <Link className="nav-link-circle right" to="/profile">
          Profile
        </Link>
      </div>
      <div
        className="subscription-card-wrapper"
        style={{ backgroundImage: `url(${image})` }}
      >
        <h1 className="subscription-title">Choose Your Plan</h1>

        <div className="plans-grid">
          {subscriptionPlans.map((card, index) => (
            <div key={index} className="plan-card">
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
                className="plan-button"
                onClick={() => handleBuySubscription(card.planType)}
              >
                Choose Plan
              </button>

              <div>
                {card.Features.map((feature, featureIndex) => (
                  <p key={featureIndex} className="plan-feature-item">
                    {feature}
                  </p>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
