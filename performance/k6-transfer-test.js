import http from "k6/http";
import { check } from "k6";

export default function () {
  const response = http.get("http://localhost:5000/health/live");

  check(response, {
    "health endpoint placeholder is reachable": (result) => result.status < 500,
  });
}

