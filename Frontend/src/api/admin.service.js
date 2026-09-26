import { api } from "@api/client.js";

export async function getTracks(page = 1, pageSize = 50) {
  const { data } = await api.get("/tracks", { params: { page, pageSize } });
  return data;
}

export async function getAlbums() {
  const { data } = await api.get("/albums");
  return data;
}

export async function createTrack(formData) {
  const { data } = await api.post("/admin/tracks", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export async function updateTrack(id, formData) {
  const { data } = await api.put(`/admin/tracks/${id}`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export async function deleteTrack(id) {
  await api.delete(`/admin/tracks/${id}`);
}

export async function createAlbum(formData) {
  const { data } = await api.post("/admin/albums", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export async function updateAlbum(id, formData) {
  const { data } = await api.put(`/admin/albums/${id}`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export async function deleteAlbum(id) {
  await api.delete(`/admin/albums/${id}`);
}

export async function getPodcasts() {
  const { data } = await api.get("/podcasts");
  return data;
}

export async function updatePodcast(id, formData) {
  const { data } = await api.put(`/admin/podcasts/${id}`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export async function deletePodcast(id) {
  await api.delete(`/admin/podcasts/${id}`);
}

export async function getUsers(search = "") {
  const { data } = await api.get("/admin/users", { params: { search } });
  return data;
}

export async function getRoles() {
  const { data } = await api.get("/admin/users/roles");
  return data;
}

export async function updateUserRole(userId, roleId) {
  const { data } = await api.put(`/admin/users/${userId}/role`, { roleId });
  return data;
}

export async function getWeb3Logs() {
  const { data } = await api.get("/admin/web3/logs");
  return data;
}

export async function manualActivateSubscription(userId, planType) {
  await api.post("/admin/web3/manual-activate", { userId, planType });
}