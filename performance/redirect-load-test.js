import http from "k6/http";
import { check } from "k6";

export const options = {
  vus: 50,
  duration: "10s",
};

const BASE_URL =
  "https://urlshortener-api.wonderfulbeach-0dbc4d22.centralindia.azurecontainerapps.io";
const SHORT_CODE = "2";

export default function () {
  const response = http.get(`${BASE_URL}/${SHORT_CODE}`, {
    redirects: 0,
  });

  check(response, {
    "status is redirect": (r) => r.status === 301 || r.status === 302,
  });
}
