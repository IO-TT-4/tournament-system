import axios from 'axios';
import { handleError } from '../helpers/ErrorHandler';

export const api = axios.create({ baseURL: import.meta.env.VITE_API_URL });

export async function getUser(id) {
  try {
    const data = api.get(`/user/${id}`);
    return data;
  } catch (error) {
    handleError(error);
  }
}

export async function getUpcomingTournaments() {
  try {
    const data = api.get(`/tournament/upcoming`);
    return data;
  } catch (error) {
    handleError(error);
  }
}

export async function getCurrentTournaments() {
  try {
    const data = api.get(`/tournament/current`);
    return data;
  } catch (error) {
    handleError(error);
  }
}

export async function getTournament(id) {
  try {
    const data = api.get(`/tournament/${id}`);
    return data;
  } catch (error) {
    handleError(error);
  }
}
