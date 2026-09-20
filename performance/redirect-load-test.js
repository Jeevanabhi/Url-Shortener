import http from "k6/http";
import { check } from "k6";

export const options = {
  vus: 50,
  duration: "1m",
};

const BASE_URL = "http://localhost:8080";
const SHORT_CODE = "8";

export default function () {
  const response = http.get(`${BASE_URL}/${SHORT_CODE}`, {
    redirects: 0,
  });

  check(response, {
    "status is redirect": (r) => r.status === 301 || r.status === 302,
  });
}
