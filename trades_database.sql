--select * from traders;
--select * from trades;

-- a) How many trades were made per product on August 2nd 2023
-- Answer: There is fifthen products, execute following query to see the output.
SELECT product_id, COUNT(*) AS trade_count
FROM trades
WHERE DATE_TRUNC('day', execution_time) = '2023-08-02'
GROUP BY product_id
ORDER BY product_id;

-- b) How much profit did we made for product 7201?
SELECT 
    SUM((sell_price - buy_price) * qty) AS total_profit
FROM (
    SELECT 
        price AS buy_price,
        LEAD(price) OVER (ORDER BY execution_time) AS sell_price,
        qty
    FROM 
        trades
    WHERE 
        own = true 
        AND deleted = false 
        AND product_id = 7201
) AS subquery;

-- c) Which traders are most active? List traders ranked by number of trades.
SELECT 
    t.name AS trader_name,
    COUNT(*) AS trade_count
FROM 
    traders t
JOIN 
    trades tr ON t.id = tr.buy_order_trader_id OR t.id = tr.sell_order_trader_id
GROUP BY 
    t.name
ORDER BY 
    COUNT(*) DESC;

-- d) We are preparing a dashboard that shows current market state. Prepare a query that retrieves last trade for each product.
WITH LatestTrades AS (
    SELECT 
        *,
        ROW_NUMBER() OVER (PARTITION BY product_id ORDER BY execution_time DESC) AS rn
    FROM 
        trades
)
SELECT 
    id,
    price,
    qty,
    execution_time,
    deleted,
    own,
    buy_order_own,
    buy_order_trader_id,
    sell_order_own,
    sell_order_trader_id,
    product_id
FROM 
    LatestTrades
WHERE 
    rn = 1;

-- e) To update the data in the database with the trades from the 'trades.csv' file, I would recommend the following steps:
/*
1. Prepare and review CSV data
Before we start adding data to the database, we should review the data in the .CSV file to check the accuracy of the data and the format of the .CSV file.
2. Backup Existing Data (optional and recommended):
It's good practice to create a backup of the existing data in the 'trades' table before performing any updates. This backup can be helpful in case of any issues during the update process.
3. Load CSV Data into temporary table:
We can use SQL commands to load the data from the 'trades.csv' file into a temporary table within the database. We need to make sure the temporary table has the same structure as the 'trades' table.
4. Identify new records:
Compare the data in the temporary table with the existing data in the 'trades' table to identify any new records or updates. We can use SQL queries to compare and identify the differences.
5. Insert new records in main table:
Now we can insert records from the temporary table into the 'trades' table. We can use SQL INSERT statements to add these records while ensuring that primary key constraints are maintained.
6. Clean up temporary table (optional):
Once the update process is complete, we remove or truncate the temporary table.
7. Testing and validation (crucial part in my opinion):
Testing and validation are crucial steps to ensure that the updated data behaves as expected and meets the required criteria. Some technical details:
 - test with queris: we can write and execute SQL queries to retrieve specific data from the 'trades' table and to verify that it matches the expected results. 
 - functional testing: we can test applications or dashboard that rely on the 'trades' data to ensure they function correctly with the updated data.
 - regression testing: since we made changes to the database schema during the update process, we need to perform regression testing to ensure that existing functionality was not adversely affected.
8. Documentation (optional)
It is recommended to document the steps we took during the update process (for example: the scripts or commands we used, for future purposes).
*/

-- f) For a mentioned portal traders would like to see OHLC candles where they can see price movement for a product in some time range. Design a database table (ohlc_1min) that stores ohlc candles at a minute level and populate the data.
CREATE TABLE ohlc_1min (
    product_id integer NOT NULL,
    time timestamp NOT NULL,
    qty numeric(38,16) NOT NULL,
    open_price numeric(38,16) NOT NULL,
    close_price numeric(38,16) NOT NULL,
    low_price numeric(38,16) NOT NULL,
    high_price numeric(38,16) NOT NULL,
    CONSTRAINT pk_ohlc_1min PRIMARY KEY (product_id, time)
);

INSERT INTO ohlc_1min (
    product_id,
    time,
    qty,
    open_price,
    close_price,
    low_price,
    high_price
)
SELECT
    product_id,
    time,
    SUM(qty) AS qty,
    MIN(open_price) AS open_price,
    MAX(close_price) AS close_price,
    MIN(price) AS low_price,
    MAX(price) AS high_price
FROM (
    SELECT
        product_id,
        date_trunc('minute', trades.execution_time) AS time,
        trades.qty,
        trades.price,
        FIRST_VALUE(trades.price) OVER(PARTITION BY product_id, date_trunc('minute', trades.execution_time) ORDER BY trades.execution_time ASC) AS open_price,
        FIRST_VALUE(trades.price) OVER(PARTITION BY product_id, date_trunc('minute', trades.execution_time) ORDER BY trades.execution_time DESC) AS close_price
    FROM
        trades
) trades_min
GROUP BY
    product_id,
    time
ORDER BY
    product_id,
    time;