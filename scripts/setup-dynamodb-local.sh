#!/bin/bash
# Script to create the DynamoDB tables for local development

# Wait for DynamoDB Local to be ready
echo "Waiting for DynamoDB Local..."
until aws dynamodb list-tables --endpoint-url http://localhost:8000 > /dev/null 2>&1; do
    sleep 1
done
echo "DynamoDB Local is ready!"

# Create the Registrants table
echo "Creating FreeIranPortal-Registrants-Dev table..."
aws dynamodb create-table \
    --table-name FreeIranPortal-Registrants-Dev \
    --attribute-definitions \
        AttributeName=Id,AttributeType=S \
        AttributeName=Email,AttributeType=S \
        AttributeName=Country,AttributeType=S \
    --key-schema \
        AttributeName=Id,KeyType=HASH \
    --global-secondary-indexes \
        "[
            {
                \"IndexName\": \"email-index\",
                \"KeySchema\": [{\"AttributeName\":\"Email\",\"KeyType\":\"HASH\"}],
                \"Projection\": {\"ProjectionType\":\"ALL\"},
                \"ProvisionedThroughput\": {\"ReadCapacityUnits\":5,\"WriteCapacityUnits\":5}
            },
            {
                \"IndexName\": \"country-index\",
                \"KeySchema\": [{\"AttributeName\":\"Country\",\"KeyType\":\"HASH\"}],
                \"Projection\": {\"ProjectionType\":\"ALL\"},
                \"ProvisionedThroughput\": {\"ReadCapacityUnits\":5,\"WriteCapacityUnits\":5}
            }
        ]" \
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
    --endpoint-url http://localhost:8000 \
    --region local

echo "Table created successfully!"
echo ""
echo "You can access DynamoDB Admin UI at: http://localhost:8001"
