
Vector3 center = _grid.CellToWorld(cellPos);
float w = sprite.bounds.size.x;
float h = sprite.bounds.size.y;
_vertices[vStart + 0] = center + new Vector3(-w / 2, -h / 2);
_vertices[vStart + 1] = center + new Vector3(-w / 2, h / 2);
_vertices[vStart + 2] = center + new Vector3(w / 2, h / 2);
_vertices[vStart + 3] = center + new Vector3(w / 2, -h / 2);

var rect = sprite.textureRect;
var tex = sprite.texture;
Vector2 uv00 = new Vector2(rect.xMin / tex.width, rect.yMin / tex.height);
Vector2 uv11 = new Vector2(rect.xMax / tex.width, rect.yMax / tex.height);
_uvs[vStart + 0] = new Vector2(uv00.x, uv00.y);
_uvs[vStart + 1] = new Vector2(uv00.x, uv11.y);
_uvs[vStart + 2] = new Vector2(uv11.x, uv11.y);
_uvs[vStart + 3] = new Vector2(uv11.x, uv00.y);

mesh.vertices = _vertices;
mesh.uv = _uvs;
mesh.RecalculateBounds();