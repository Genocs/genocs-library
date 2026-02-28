Feature: Saga Transaction Management
    As a system administrator
    I want to manage saga transactions
    So that I can ensure distributed transactions are properly coordinated

Background:
    Given the saga transaction service is initialized

Scenario: Start a new saga transaction successfully
    Given I have transaction details with text "Process payment" and originator "PaymentService"
    When I start a new saga transaction
    Then a new saga should be created
    And the saga should have a valid SagaId
    And the saga context should contain the originator "PaymentService"

Scenario: Complete an existing saga transaction successfully
    Given I have started a saga transaction with text "Process order" and originator "OrderService"
    When I complete the saga transaction with text "Order completed" and originator "OrderService"
    Then the saga should be marked as completed
    And the completion should be logged

Scenario: Complete saga transaction with rejection
    Given I have started a saga transaction with text "Process refund" and originator "RefundService"
    When I complete the saga transaction that results in rejection
    Then the saga should be marked as rejected
    And the rejection should be logged with error level

Scenario Outline: Start multiple saga transactions with different parameters
    Given I have transaction details with text "<TransactionText>" and originator "<Originator>"
    When I start a new saga transaction
    Then a new saga should be created
    And the saga context should contain the originator "<Originator>"

Examples:
    | TransactionText   | Originator          |
    | Process payment   | PaymentService      |
    | Create order      | OrderService        |
    | Send notification | NotificationService |
    | Update inventory  | InventoryService    |