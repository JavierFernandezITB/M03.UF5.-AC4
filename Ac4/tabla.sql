CREATE TABLE IF NOT EXISTS Records (
	Id SERIAL PRIMARY KEY,
	Year int,
	CodiComarca int,
	Comarca varchar(32),
	Poblacio int,
	DomesticXarxa int,
	ActividadesEconomicas int,
	Total int,
	ConsumoCapita decimal
);