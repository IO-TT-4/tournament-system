import { api } from '../api/api';
import { handleError } from '../helpers/ErrorHandler';

export async function loginAPI(username: string, password: string) {
  try {
    const data = api.post('/auth/login', {
      username: username,
      password: password,
    });
    return data;
  } catch (error) {
    handleError(error);
  }
}

export async function registerApi(
  email: string,
  username: string,
  password: string,
) {
  try {
    const data = api.post('/auth/register', {
      username: username,
      password: password,
      email: email,
    });
    return data;
  } catch (error) {
    handleError(error);
  }
}

export interface TournamentParams {
  searchTerm?: string;
  discipline?: string;
  status?: string;
  location?: {
    city?: string;
    lat?: number;
    lng?: number;
    radius?: number; // km
  };
  sortBy?: string;
  page?: number;
  limit?: number;
}

export interface Tournament {
  id: string;
  title: string;
  date: string;
  emblem: string;
  game: {
    name: string;
    code: string;
  };
  location: string;
  details: string;
  status: 'active' | 'upcoming' | 'completed';
  lat: number | null;

  lng: number | null;
}



export async function getTournaments(params: TournamentParams = {}) {
  // Map frontend status to backend enum string
  const statusMap: Record<string, string> = {
    active: 'ONGOING',
    upcoming: 'CREATED',
    completed: 'FINISHED',
  };

  // Flatten and map query params to match backend TournamentFilterParams
  const queryParams = {
    SearchTerm: params.searchTerm,
    GameCode: params.discipline !== 'all' ? params.discipline : undefined,
    Status: params.status && params.status !== 'all' ? statusMap[params.status] : undefined,
    City: params.location?.city !== 'all' ? params.location?.city : undefined,
    Lat: params.location?.lat,
    Lng: params.location?.lng,
    Radius: params.location?.radius,
    SortBy: params.sortBy || 'relevance',
    Page: params.page || 1,
    Limit: params.limit || 10,
  };

  const response = await api.get('/tournament', { params: queryParams });
  const backendData = response.data;

  // Map backend response to frontend Tournament interface
  const tournaments: Tournament[] = backendData.data.map((t: any) => ({
    id: t.id,
    title: t.name,
    date: new Date(t.startDate).toLocaleDateString(),
    emblem: t.emblem || 'default',
    game: {
      name: t.gameName || 'Other',
      code: t.gameCode || 'all',
    },
    location: t.city || 'Online',
    details: `Organizer: ${t.organizerName}`,
    status: t.status === 'ONGOING' ? 'active' : t.status === 'FINISHED' ? 'completed' : 'upcoming',
    lat: t.lat,
    lng: t.lng,
  }));

  return {
    data: tournaments,
    total: backendData.totalCount,
    page: backendData.page,
    totalPages: Math.ceil(backendData.totalCount / backendData.limit),
  };
}

export async function getTournamentById(id: string): Promise<Tournament | null> {
  try {
    const response = await api.get(`/tournament/${id}`);
    const t = response.data;
    return {
      id: t.id,
      title: t.name,
      date: new Date(t.startDate).toLocaleDateString(),
      emblem: t.emblem || 'default',
      game: {
        name: t.gameName || 'Other',
        code: t.gameCode || 'all',
      },
      location: t.city || 'Online',
      details: `Organizer: ${t.organizerName}`,
      status: t.status === 'ONGOING' ? 'active' : t.status === 'FINISHED' ? 'completed' : 'upcoming',
      lat: t.lat,
      lng: t.lng,
    };
  } catch (error) {
    handleError(error);
    return null;
  }
}

export async function trackTournamentActivity(id: string, type: 'view' | 'click' | 'join' = 'view') {
  try {
    await api.post(`/tournament/${id}/track?type=${type}`);
  } catch {
    // Silent fail for stats
  }
}

