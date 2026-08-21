import React from "react";
import image from "../assets/images/register-bg.png";
import { buySubscription } from "../utilites/blockchainUtils";
import { AccountContext } from "../contexts/account.context";

export const PLAN_TYPE = Object.freeze({
  INDIVIDUAL: 0,
  DUO: 1,
  FAMILY: 2,
});

export default function SubscriptionPage() {
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
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        width: "1080px",
        backgroundColor: "#121212",
      }}
    >
      <div
        style={{
          width: "1720px",
          height: "847px",
          position: "absolute",
          top: "100px",
          left: "100px",
          borderRadius: "8px",
          backgroundImage: `url(${image})`,
          textAlign: "center",
        }}
      >
        <h1
          style={{
            width: "882px",
            height: "68px",
            position: "absolute",
            top: "46px",
            left: "419px",
            fontWeight: "800",
            fontSize: "50px",
            lineHeight: "130%",
            color: "#FFFFFF",
            textAlign: "center",
            fontFamily: "Inter",
          }}
        >
          Choose Your Plan
        </h1>

        <div
          style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            gap: "40px",
            width: "950px",
            height: "650px",
            position: "absolute",
            top: "150px",
            left: "385px",
          }}
        >
          {subscriptionPlans.map((card, index) => (
            <div
              key={index}
              style={{
                width: "290px",
                height: "650px",
                borderRadius: "20px",
                border: "2px solid black",
                color: "#F5F5F5",
                backgroundColor: "#F5F5F5",
                padding: "0 25px 0 25px",
                gap: "10px",
                textAlign: "left",
              }}
            >
              <div>
                <p
                  style={{
                    width: "240px",
                    height: "25px",
                    fontWeight: "bold",
                    fontSize: "25px",
                    lineHeight: "100%",
                    color: "#1F1F1F",
                    fontFamily: "Inter",
                    paddingTop: "20px",
                    marginBottom: "25px",
                  }}
                >
                  {card.Name}
                </p>
                <p
                  style={{
                    width: "240px",
                    height: "12px",
                    fontWeight: "500",
                    fontSize: "12px",
                    lineHeight: "100%",
                    color: "#1F1F1F",
                    fontFamily: "Inter",
                  }}
                >
                  {card.Accounts}
                </p>
              </div>

              <div>
                <p
                  style={{
                    width: "240px",
                    height: "36px",
                    fontWeight: "Bold",
                    fontSize: "36px",
                    lineHeight: "100%",
                    color: "#1F1F1F",
                    fontFamily: "Inter",
                  }}
                >
                  ${card.Price}
                  <span
                    style={{
                      fontSize: "16px",
                      color: "#AEAEAE",
                      marginBottom: "0px",
                      fontFamily: "Inter",
                      fontWeight: "normal",
                    }}
                  >
                    {" "}
                    /month
                  </span>
                </p>

                <p
                  style={{
                    width: "240px",
                    height: "12px",
                    fontWeight: "500",
                    fontSize: "12px",
                    lineHeight: "100%",
                    color: "#1F1F1F",
                    fontFamily: "Inter",
                    marginTop: "-30px",
                  }}
                >
                  Billed monthly
                </p>

                <hr
                  style={{
                    border: "1px solid #AEAEAE",
                    margin: "20px 0 10px 0",
                    width: "222px",
                  }}
                />

                <p
                  style={{
                    width: "240px",
                    height: "48px",
                    fontWeight: "500",
                    fontSize: "12px",
                    lineHeight: "130%",
                    color: "#1F1F1F",
                    fontFamily: "Inter",
                  }}
                >
                  {card.SubscriptionDescription}
                </p>
              </div>

              <button
                style={{
                  width: "240px",
                  height: "31px",
                  borderRadius: "8px",
                  cursor: "pointer",
                  backgroundColor: "#1F1F1F",
                  color: "#FFFFFF",
                  marginBottom: "20px",
                  border: "none",
                  fontFamily: "Inter",
                }}
                onClick={() => handleBuySubscription(card.planType)}
              >
                Choose Plan
              </button>

              <div>
                {card.Features.map((feature, featureIndex) => (
                  <p
                    key={featureIndex}
                    style={{
                      width: "auto",
                      height: "12px",
                      fontWeight: "500",
                      fontSize: "12px",
                      lineHeight: "100%",
                      color: "#1F1F1F",
                      fontFamily: "Inter",
                      margin: "10px 0",
                    }}
                  >
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
