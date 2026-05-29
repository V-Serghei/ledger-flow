import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    statement_reads: {
      executor: "constant-vus",
      vus: 20,
      duration: "1m",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<300"],
  },
};

const baseUrl = __ENV.BASE_URL || "http://localhost:5000";
const token = __ENV.TOKEN;
const accountId = __ENV.ACCOUNT_ID;

export default function () {
  const response = http.get(
    `${baseUrl}/api/v1/accounts/${accountId}/statement?page=1&pageSize=20`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        "X-Correlation-ID": `k6-statement-${__VU}-${__ITER}`,
      },
    },
  );

  check(response, {
    "statement returned successfully": (result) => result.status === 200,
  });

  sleep(1);
}
