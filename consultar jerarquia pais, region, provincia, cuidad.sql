-- Verificar la jerarquía completa y correcta
WITH UbicacionJerarquia AS (
    SELECT 
        Id,
        Nombre,
        Tipo,
        FK_Padre,
        0 AS Nivel,
        CAST(Nombre AS VARCHAR(500)) AS Ruta
    FROM Ubicaciones_Geograficas 
    WHERE FK_Padre IS NULL
    
    UNION ALL
    
    SELECT 
        u.Id,
        u.Nombre,
        u.Tipo,
        u.FK_Padre,
        uj.Nivel + 1,
        CAST(uj.Ruta + ' > ' + u.Nombre AS VARCHAR(500))
    FROM Ubicaciones_Geograficas u
    INNER JOIN UbicacionJerarquia uj ON u.FK_Padre = uj.Id
)
SELECT 
    REPLICATE('  ', Nivel) + Nombre AS Nombre_Indentado,
    Tipo,
    Nivel,
    Ruta
FROM UbicacionJerarquia
ORDER BY Ruta;