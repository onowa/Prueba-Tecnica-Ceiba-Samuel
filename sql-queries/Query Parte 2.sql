SELECT DISTINCT 
    c.nombre,
    c.apellidos
FROM Cliente c
WHERE EXISTS (
    SELECT 1
    FROM Inscripcion i
    INNER JOIN Producto p ON i.idProducto = p.id
    WHERE i.idCliente = c.id
    AND EXISTS (
        SELECT 1
        FROM Disponibilidad d
        INNER JOIN Visitan v ON d.idSucursal = v.idSucursal
        WHERE d.idProducto = p.id 
        AND v.idCliente = c.id
    )
    AND NOT EXISTS (
        SELECT 1
        FROM Disponibilidad d2
        WHERE d2.idProducto = p.id
        AND d2.idSucursal NOT IN (
            SELECT v2.idSucursal
            FROM Visitan v2
            WHERE v2.idCliente = c.id
        )
    )
);