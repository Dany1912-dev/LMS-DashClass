import { useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { API } from "../api";

export default function MicrosoftHandler() {
  const { instance } = useMsal();

  useEffect(() => {
    instance.handleRedirectPromise().then(async (result) => {
      if (result && result.idToken) {
        try {
          const response = await fetch(API.microsoftLogin, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idToken: result.idToken }),
          });
          if (response.ok) {
            const data = await response.json();
            localStorage.setItem("token", data.data.accessToken);
            localStorage.setItem("refreshToken", data.data.refreshToken);
            localStorage.setItem("usuario", JSON.stringify(data.data.usuario));
            window.location.href = "/dashboard";
          }
        } catch (err) {
          console.error(err);
        }
      }
    });
  }, []);

  return null;
}
