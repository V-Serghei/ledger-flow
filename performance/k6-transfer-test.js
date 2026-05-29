import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    moderate_transfer_load: {
      executor: "constant-vus",
      vus: 10,
      duration: "1m",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.05"],
    http_req_duration: ["p(95)<500"],
  },
};

const baseUrl = __ENV.BASE_URL || "http://localhost:5000";
const token = __ENV.TOKEN;
const sourceAccountId = __ENV.SOURCE_ACCOUNT_ID;
const destinationAccountId = __ENV.DESTINATION_ACCOUNT_ID;

export default function () {
  const payload = JSON.stringify({
    sourceAccountId,
    destinationAccountId,
    amount: 1.0,
    description: "k6 transfer scenario",
  });

  const response = http.post(`${baseUrl}/api/v1/transfers`, payload, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      "Idempotency-Key": `k6-transfer-${__VU}-${__ITER}`,
      "X-Correlation-ID": `k6-transfer-${__VU}-${__ITER}`,
    },
  });

  check(response, {
    "transfer accepted or business conflict": (result) =>
      result.status === 201 || result.status === 409,
  });

  sleep(1);
}
