CREATE DATABASE `wanadi`;

--DROP TABLE `wanadi`.`wanadi_test`;

CREATE TABLE wanadi.wanadi_test
(
    `id` UUID,
    `name` String NOT NULL,
    `birth_date` Date NULL, 
    `is_active` Boolean NOT NULL, 
    `wage` Decimal(18,2) NOT NULL,
	`age` INT NOT NULL, 
    `created_at` DateTime DEFAULT now()
)
ENGINE = MergeTree
PRIMARY KEY id
ORDER BY id
SETTINGS index_granularity = 8192;