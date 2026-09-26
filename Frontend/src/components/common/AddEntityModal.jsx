import React, { useState } from "react";
import { AccountContext } from "@contexts/account.store";
import "@css/AddEntityModal.css";

export default function AddEntityModal({
  entityId,
  entityType,
  onClose,
  onSuccess,
  apiEndpointBuilder,
}) {
  const [elementName, setElementName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const { accessToken } = React.useContext(AccountContext);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const headers = {
        "Content-Type": "application/json",
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      };

      const endpoint = apiEndpointBuilder(entityId, elementName);

      const response = await fetch(endpoint, {
        method: "POST",
        headers: headers,
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Помилка при виконанні запиту");
      }

      const contentType = response.headers.get("content-type");
      let updatedEntity = null;
      if (contentType && contentType.includes("application/json")) {
        updatedEntity = await response.json();
      }

      if (onSuccess) {
        onSuccess(updatedEntity);
      }
      onClose();
    } catch (err) {
      console.error(err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        <h2>
          {entityType == "Podcast" ? "Add Episode to" : "Add Track to"} {entityType}
        </h2>

        {error && (
          <div
            className="modal-error"
            style={{ color: "red", marginBottom: "10px" }}
          >
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder={
              entityType == "Podcast"
                ? "Enter Episode Name..."
                : "Enter Track Name..."
            }
            value={elementName}
            onChange={(e) => setElementName(e.target.value)}
            required
          />

          <div className="modal-actions" style={{ marginTop: "20px" }}>
            <button type="button" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" disabled={loading}>
              {loading ? "Adding..." : "Add"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
