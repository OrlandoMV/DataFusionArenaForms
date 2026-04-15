using System;
using System.Collections.Generic;

namespace DataFusionArena
{
    public class DataProcessor
    {
        private readonly List<DataItem> items = new List<DataItem>();
        private readonly Dictionary<int, DataItem> idLookup = new Dictionary<int, DataItem>();
        private readonly Dictionary<string, List<DataItem>> categoryLookup = new Dictionary<string, List<DataItem>>(StringComparer.OrdinalIgnoreCase);

        // IDs generados positivos cuando faltan o hay duplicados
        private int nextGeneratedId = 1;

        public void AgregarDatos(List<DataItem> nuevosDatos)
        {
            if (nuevosDatos == null) return;

            // Inicializar nextGeneratedId como max existente + 1
            int maxId = 0;
            foreach (var k in idLookup.Keys)
            {
                if (k > maxId) maxId = k;
            }
            nextGeneratedId = maxId + 1;

            foreach (var ni in nuevosDatos)
            {
                if (ni == null) continue;

                // Si Id no es válido (<=0) o ya existe en el diccionario, asignar Id generado positivo
                if (ni.Id <= 0 || idLookup.ContainsKey(ni.Id))
                {
                    // Buscar nextGeneratedId libre
                    while (idLookup.ContainsKey(nextGeneratedId))
                    {
                        nextGeneratedId++;
                    }
                    ni.Id = nextGeneratedId;
                    nextGeneratedId++;
                }

                // Ahora podemos insertar de forma segura
                items.Add(ni);
                idLookup[ni.Id] = ni;

                // Agrupar por categoría (uso estricto de diccionarios y bucles)
                var key = ni.Categoria ?? string.Empty;
                if (!categoryLookup.ContainsKey(key))
                {
                    categoryLookup[key] = new List<DataItem>();
                }
                categoryLookup[key].Add(ni);
            }
        }

        public List<DataItem> FiltrarPorCategoriaManual(string categoria)
        {
            var result = new List<DataItem>();
            if (string.IsNullOrEmpty(categoria)) return result;
            foreach (var kvp in categoryLookup)
            {
                if (string.Equals(kvp.Key, categoria, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var it in kvp.Value)
                    {
                        result.Add(it);
                    }
                }
            }
            return result;
        }

        // Filtrar por cualquier campo sencillo (Id, Nombre, Categoria, Precio)
        public List<DataItem> FiltrarPorCampo(string campo, string valor)
        {
            var res = new List<DataItem>();
            if (string.IsNullOrEmpty(campo)) return res;
            campo = campo.Trim();
            valor = valor ?? string.Empty;
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                if (string.Equals(campo, "Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(valor, out int vi) && it.Id == vi) res.Add(it);
                }
                else if (string.Equals(campo, "Nombre", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    if ((it.Nombre ?? string.Empty).IndexOf(valor, StringComparison.OrdinalIgnoreCase) >= 0) res.Add(it);
                }
                else if (string.Equals(campo, "Categoria", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Category", StringComparison.OrdinalIgnoreCase))
                {
                    if ((it.Categoria ?? string.Empty).IndexOf(valor, StringComparison.OrdinalIgnoreCase) >= 0) res.Add(it);
                }
                else if (string.Equals(campo, "Precio", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Price", StringComparison.OrdinalIgnoreCase))
                {
                    // allow value like ">=100" ">100" "<50" or exact number
                    var v = valor.Trim();
                    if (string.IsNullOrEmpty(v)) continue;
                    try
                    {
                        if (v.StartsWith(">=")) { if (decimal.TryParse(v.Substring(2), out decimal dv) && it.Precio >= dv) res.Add(it); }
                        else if (v.StartsWith("<=")) { if (decimal.TryParse(v.Substring(2), out decimal dv) && it.Precio <= dv) res.Add(it); }
                        else if (v.StartsWith(">")) { if (decimal.TryParse(v.Substring(1), out decimal dv) && it.Precio > dv) res.Add(it); }
                        else if (v.StartsWith("<")) { if (decimal.TryParse(v.Substring(1), out decimal dv) && it.Precio < dv) res.Add(it); }
                        else { if (decimal.TryParse(v, out decimal dv) && it.Precio == dv) res.Add(it); }
                    }
                    catch { }
                }
            }
            return res;
        }

        public void OrdenarPorPrecioManual(bool ascendente)
        {
            // Use bubble sort
            for (int i = 0; i < items.Count - 1; i++)
            {
                for (int j = 0; j < items.Count - 1 - i; j++)
                {
                    bool swap = false;
                    if (ascendente)
                    {
                        if (items[j].Precio > items[j + 1].Precio) swap = true;
                    }
                    else
                    {
                        if (items[j].Precio < items[j + 1].Precio) swap = true;
                    }
                    if (swap)
                    {
                        var tmp = items[j];
                        items[j] = items[j + 1];
                        items[j + 1] = tmp;
                    }
                }
            }
        }

        // Ordenar por campo (Id, Nombre, Categoria, Precio) sin usar LINQ
        public void OrdenarPorCampoManual(string campo, bool ascendente)
        {
            if (string.IsNullOrEmpty(campo)) return;
            campo = campo.Trim();
            // Simple bubble sort comparing based on field
            for (int i = 0; i < items.Count - 1; i++)
            {
                for (int j = 0; j < items.Count - 1 - i; j++)
                {
                    var a = items[j];
                    var b = items[j + 1];
                    if (a == null || b == null) continue;
                    bool shouldSwap = false;
                    if (string.Equals(campo, "Id", StringComparison.OrdinalIgnoreCase))
                    {
                        shouldSwap = ascendente ? (a.Id > b.Id) : (a.Id < b.Id);
                    }
                    else if (string.Equals(campo, "Nombre", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Name", StringComparison.OrdinalIgnoreCase))
                    {
                        int cmp = string.Compare(a.Nombre ?? string.Empty, b.Nombre ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                        shouldSwap = ascendente ? (cmp > 0) : (cmp < 0);
                    }
                    else if (string.Equals(campo, "Categoria", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Category", StringComparison.OrdinalIgnoreCase))
                    {
                        int cmp = string.Compare(a.Categoria ?? string.Empty, b.Categoria ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                        shouldSwap = ascendente ? (cmp > 0) : (cmp < 0);
                    }
                    else if (string.Equals(campo, "Precio", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Price", StringComparison.OrdinalIgnoreCase))
                    {
                        shouldSwap = ascendente ? (a.Precio > b.Precio) : (a.Precio < b.Precio);
                    }
                    if (shouldSwap)
                    {
                        var tmp = items[j];
                        items[j] = items[j + 1];
                        items[j + 1] = tmp;
                    }
                }
            }
        }

        // Agrupar por campo (devuelve un diccionario con clave string y lista de items)
        public Dictionary<string, List<DataItem>> AgruparPorCampoManual(string campo)
        {
            var dict = new Dictionary<string, List<DataItem>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(campo)) return dict;
            campo = campo.Trim();
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                string key = string.Empty;
                if (string.Equals(campo, "Id", StringComparison.OrdinalIgnoreCase)) key = it.Id.ToString();
                else if (string.Equals(campo, "Nombre", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Name", StringComparison.OrdinalIgnoreCase)) key = it.Nombre ?? string.Empty;
                else if (string.Equals(campo, "Categoria", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Category", StringComparison.OrdinalIgnoreCase)) key = it.Categoria ?? string.Empty;
                else if (string.Equals(campo, "Precio", StringComparison.OrdinalIgnoreCase) || string.Equals(campo, "Price", StringComparison.OrdinalIgnoreCase)) key = it.Precio.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (!dict.ContainsKey(key)) dict[key] = new List<DataItem>();
                dict[key].Add(it);
            }
            return dict;
        }

        // Detectar duplicados por contenido (Nombre+Categoria+Precio). Devuelve grupos con más de 1 elemento.
        public List<List<DataItem>> DetectarDuplicadosPorContenido()
        {
            var groups = new Dictionary<string, List<DataItem>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                var key = (it.Nombre ?? string.Empty).Trim().ToLowerInvariant() + "||" + (it.Categoria ?? string.Empty).Trim().ToLowerInvariant() + "||" + it.Precio.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (!groups.ContainsKey(key)) groups[key] = new List<DataItem>();
                groups[key].Add(it);
            }
            var res = new List<List<DataItem>>();
            foreach (var kvp in groups)
            {
                if (kvp.Value.Count > 1) res.Add(kvp.Value);
            }
            return res;
        }

        // Detectar duplicados por Id (si existieran)
        public List<List<DataItem>> DetectarDuplicadosPorId()
        {
            var groups = new Dictionary<int, List<DataItem>>();
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                if (!groups.ContainsKey(it.Id)) groups[it.Id] = new List<DataItem>();
                groups[it.Id].Add(it);
            }
            var res = new List<List<DataItem>>();
            foreach (var kvp in groups)
            {
                if (kvp.Value.Count > 1) res.Add(kvp.Value);
            }
            return res;
        }

        public List<DataItem> GetAllItems()
        {
            var copy = new List<DataItem>();
            foreach (var it in items) copy.Add(it);
            return copy;
        }

        public Dictionary<string, List<DataItem>> GetCategoryDictionary()
        {
            // Return a shallow copy
            var copy = new Dictionary<string, List<DataItem>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in categoryLookup)
            {
                var list = new List<DataItem>();
                foreach (var it in kvp.Value) list.Add(it);
                copy[kvp.Key] = list;
            }
            return copy;
        }
    }
}
