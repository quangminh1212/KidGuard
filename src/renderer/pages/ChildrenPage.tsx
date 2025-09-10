import React from 'react';
import { Box, Typography, Card, CardContent, Button, Grid } from '@mui/material';
import { Add, ChildCare } from '@mui/icons-material';

const ChildrenPage: React.FC = () => {
  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Children Profiles
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage your children's profiles and monitoring settings.
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<Add />}>
          Add Child
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <ChildCare sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" gutterBottom>
                No Children Profiles
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Add your first child profile to start monitoring their digital activities.
              </Typography>
              <Button variant="contained" startIcon={<Add />}>
                Add Child Profile
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ChildrenPage;
