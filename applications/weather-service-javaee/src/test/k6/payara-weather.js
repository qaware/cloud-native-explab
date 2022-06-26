import {check, group} from "k6";
import http from "k6/http";

export let options = {
    thresholds: {
        'http_req_duration{kind:html}': ["avg<=500"],
        'http_req_duration{kind:rest}': ["avg<=1000"],
        'http_req_duration{kind:admin}': ["avg<=500"],
    }
};

export default function () {
    group("static", function () {
        check(http.get("http://localhost:8080", {
            tags: {'kind': 'html'},
        }), {
            "status is 200": (res) => res.status === 200,
        });
    });

    group("rest", function () {
        check(http.get("http://localhost:8080/api/weather?city=Rosenheim", {
            headers: {"Content-Type": "application/json", "Accept": "application/json"},
            tags: {'kind': 'rest'},
        }), {
            "status is 200": (res) => res.status === 200,
        });

        check(http.get("http://localhost:8080/api/weather?city=London", {
            headers: {"Content-Type": "application/json", "Accept": "application/json"},
            tags: {'kind': 'rest'},
        }), {
            "status is 200": (res) => res.status === 200,
        });

        check(http.get("http://localhost:8080/api/weather?city=Munich", {
            headers: {"Content-Type": "application/json", "Accept": "application/json"},
            tags: {'kind': 'rest'},
        }), {
            "status is 200": (res) => res.status === 200,
        });
    });

    group("admin", function () {
        check(http.get("http://localhost:8080/health", {
            headers: {"Content-Type": "application/json", "Accept": "application/json"},
            tags: {'kind': 'admin'},
        }), {
            "status is 200": (res) => res.status === 200,
        });

        check(http.get("http://localhost:8080/metrics", {
            headers: {"Content-Type": "application/json", "Accept": "application/json"},
            tags: {'kind': 'admin'},
        }), {
            "status is 200": (res) => res.status === 200,
        });
    });
}