Put the PostgreSQL script of Part 1 here.

CREATE DATABASE Part;


\c Part


CREATE TABLE item (
    id SERIAL PRIMARY KEY,
    item_name VARCHAR(50) NOT NULL,
    parent_item INTEGER REFERENCES item(id),
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL
);


INSERT INTO item (item_name, parent_item, cost, req_date) VALUES
    ('Item1', null, 500, '2024-02-20'),
    ('Sub1', 1, 200, '2024-02-10'),
    ('Sub2', 1, 300, '2024-01-05'),
    ('Sub3', 2, 300, '2024-01-02'),
    ('Sub4', 2, 400, '2024-01-02'),
    ('Item2', null, 600, '2024-03-15'),
    ('Sub1', 6, 200, '2024-02-25');


CREATE OR REPLACE FUNCTION Get_Total_Cost(item_name VARCHAR(50))
RETURNS INTEGER AS $$
DECLARE
    total_cost INTEGER;
BEGIN
    WITH RECURSIVE cte AS (
        SELECT id, item_name, cost
        FROM item
        WHERE item_name = Get_Total_Cost.item_name
            AND parent_item IS NULL
        UNION ALL
        SELECT i.id, i.item_name, i.cost
        FROM item i
            JOIN cte c ON i.parent_item = c.id
    )
    SELECT sum(cost) INTO total_cost FROM cte;

    RETURN total_cost;
END;
$$ LANGUAGE plpgsql;