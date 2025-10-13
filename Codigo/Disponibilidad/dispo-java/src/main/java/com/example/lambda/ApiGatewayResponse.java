package com.example.lambda;

import java.util.Collections;
import java.util.Map;

public class ApiGatewayResponse {
    private int statusCode;
    private String body;
    private Map<String, String> headers;

    public ApiGatewayResponse(int statusCode, String body) {
        this.statusCode = statusCode;
        this.body = body;
        this.headers = Collections.singletonMap("Content-Type", "application/json");
    }

    public int getStatusCode() { return statusCode; }
    public String getBody() { return body; }
    public Map<String, String> getHeaders() { return headers; }
}
