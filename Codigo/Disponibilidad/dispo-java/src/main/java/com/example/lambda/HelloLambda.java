package com.example.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;

import java.util.Map;

public class HelloLambda implements RequestHandler<Map<String, Object>, ApiGatewayResponse> {

    @Override
    public ApiGatewayResponse handleRequest(Map<String, Object> input, Context context) {
        context.getLogger().log("Input: " + input);

        String name = "World";
        if (input != null && input.get("queryStringParameters") != null) {
            Map<String, String> query = (Map<String, String>) input.get("queryStringParameters");
            name = query.getOrDefault("name", "World");
        }

        String message = String.format("Hello, %s!", name);
        return new ApiGatewayResponse(200, message);
    }
}
