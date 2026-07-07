import { createAsyncThunk, createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { authApi } from "./api/authApi";
import type { UserProfile } from "@/types/auth";
import { toApiError } from "@/shared/lib/apiClient";

interface AuthState {
  user: UserProfile | null;
  status: "idle" | "loading" | "authenticated" | "unauthenticated";
  twoFactorChallengeToken: string | null;
}

const initialState: AuthState = {
  user: null,
  status: "idle",
  twoFactorChallengeToken: null,
};

export const bootstrapAuth = createAsyncThunk("auth/bootstrap", async () => {
  return authApi.me();
});

export const login = createAsyncThunk(
  "auth/login",
  async (data: { email: string; password: string; rememberMe: boolean }, { rejectWithValue }) => {
    try {
      return await authApi.login(data);
    } catch (err) {
      return rejectWithValue(toApiError(err).message);
    }
  },
);

export const verifyTwoFactorLogin = createAsyncThunk(
  "auth/verifyTwoFactorLogin",
  async (data: { challengeToken: string; code: string; rememberMe: boolean }, { rejectWithValue }) => {
    try {
      return await authApi.verifyTwoFactorLogin(data);
    } catch (err) {
      return rejectWithValue(toApiError(err).message);
    }
  },
);

export const logout = createAsyncThunk("auth/logout", async () => {
  await authApi.logout();
});

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    userUpdated(state, action: PayloadAction<UserProfile>) {
      state.user = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(bootstrapAuth.pending, (state) => {
        state.status = "loading";
      })
      .addCase(bootstrapAuth.fulfilled, (state, action) => {
        state.user = action.payload;
        state.status = "authenticated";
      })
      .addCase(bootstrapAuth.rejected, (state) => {
        state.user = null;
        state.status = "unauthenticated";
      })
      .addCase(login.pending, (state) => {
        state.status = "loading";
      })
      .addCase(login.fulfilled, (state, action) => {
        if (action.payload.requiresTwoFactor) {
          state.twoFactorChallengeToken = action.payload.challengeToken ?? null;
          state.status = "unauthenticated";
        } else {
          state.user = action.payload.profile ?? null;
          state.twoFactorChallengeToken = null;
          state.status = "authenticated";
        }
      })
      .addCase(login.rejected, (state) => {
        state.status = "unauthenticated";
      })
      .addCase(verifyTwoFactorLogin.fulfilled, (state, action) => {
        state.user = action.payload.profile;
        state.twoFactorChallengeToken = null;
        state.status = "authenticated";
      })
      .addCase(logout.fulfilled, (state) => {
        state.user = null;
        state.status = "unauthenticated";
      });
  },
});

export const { userUpdated } = authSlice.actions;
export default authSlice.reducer;
